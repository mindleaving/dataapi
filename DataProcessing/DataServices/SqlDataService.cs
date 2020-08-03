using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SqlMigrationTools;
using Commons.Misc;
using DataProcessing.Models;
using Newtonsoft.Json.Linq;

namespace DataProcessing.DataServices
{
    public class SqlDataService : IDataService
    {
        public const string DataApiIdColumnName = "DataApiId";

        private readonly SqlDataServiceTarget target;
        private readonly string tablePath;
        private readonly SqlMigration sqlMigration;
        private readonly KeyValuePairExtractor keyValuePairExtractor;
        private readonly object adminLock = new object();

        public SqlDataService(SqlDataServiceTarget target)
        {
            if(target.DataSource != "dkhoeinnsql01")
                throw new ArgumentOutOfRangeException(nameof(target.DataSource), $"SQL data source '{target.DataSource}' is currently not supported");
            if(target.DatabaseName != "PowerBiData")
                throw new ArgumentOutOfRangeException(nameof(target.DatabaseName), $"SQL database '{target.DatabaseName}' on {target.DataSource} is currently not supported");
            this.target = target;
            tablePath = GetTablePath(target);
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = target.DataSource,
                InitialCatalog = target.DatabaseName,
                IntegratedSecurity = false,
                UserID = target.Username,
                Password = Secrets.Get($"DataService_Sql_Password_{target.Username.ToLowerInvariant()}")
            }.ConnectionString;
            sqlMigration = new SqlMigration(sqlConnectionString);
            keyValuePairExtractor = new KeyValuePairExtractor();
        }

        public async Task InitializeAsync()
        {
            await CreateUserIfNotExistsAsync(target.Username);
            await CreateTableIfNotExistsAsync(target.Username, target.TableName);
        }

        public async Task TransferAsync(JObject jObject, List<DataServiceDefinition.Field> fields, string id)
        {
            var requiredColumns = fields.Select(x => x.As).Concat(new[] {DataApiIdColumnName});
            await EnsureColumnsExistAsync(target.DatabaseName, target.Username, target.TableName, requiredColumns);
            var exists = await sqlMigration.ExistsAsync(tablePath, $"{DataApiIdColumnName} = '{id}'");
            var query = exists ? BuildUpdateQuery(jObject, fields, id) : BuildInsertQuery(jObject, fields, id);
            await sqlMigration.RunCommandAsync(query);
        }

        public Task DeleteAsync(string id)
        {
            return sqlMigration.DeleteMatchingAsync(tablePath, $"{DataApiIdColumnName} = '{id}'");
        }

        public async Task<bool> ObjectExistsAsync(string id)
        {
            if (!await TableExistsAsync(target.Username, target.TableName))
                return false;
            return await sqlMigration.ExistsAsync(tablePath, $"DataApiId = '{id}'");
        }

        private string BuildInsertQuery(JObject jObject, List<DataServiceDefinition.Field> fields, string id)
        {
            var kvps = keyValuePairExtractor.Extract(jObject, fields)
                .Concat(new []{ new KeyValuePair<string, string>(DataApiIdColumnName, id) })
                .ToList();
            return $"INSERT INTO {tablePath} "
                   + $"({string.Join(",", kvps.Select(kvp => kvp.Key))}) "
                   + $"VALUES ({string.Join(",", kvps.Select(kvp => FormatValue(kvp.Value)))})";
        }

        private string BuildUpdateQuery(JObject jObject, List<DataServiceDefinition.Field> fields, string id)
        {
            var kvps = keyValuePairExtractor.Extract(jObject, fields);
            var setArguments = string.Join(",", kvps.Select(kvp => $"{kvp.Key}={FormatValue(kvp.Value)}"));
            return $"UPDATE {tablePath} SET {setArguments} WHERE {DataApiIdColumnName} = '{id}'";
        }

        private string FormatValue(string value)
        {
            if (value == null)
                return "NULL";
            return $"'{value}'";
        }

        private static string GetTablePath(SqlDataServiceTarget target)
        {
            return $"[{target.DatabaseName}].[{target.Username}].[{target.TableName}]";
        }

        private async Task<bool> TableExistsAsync(string schema, string tableName)
        {
            return await sqlMigration.ExistsAsync(
                $"{target.DatabaseName}.INFORMATION_SCHEMA.TABLES", 
                $"TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}'");
        }

        private async Task CreateUserIfNotExistsAsync(string username)
        {
            var existingLogin = await LoginExistsAsync(username);
            if(!existingLogin)
                await sqlMigration.RunCommandAsync($"CREATE LOGIN [GLOBAL\\{username}] FROM WINDOWS");

            var existingUser = await UserExistsAsync(username);
            if(!existingUser)
                await sqlMigration.RunCommandAsync($"CREATE USER [GLOBAL\\{username}] FOR LOGIN [GLOBAL\\{username}]");

            var existingSchema = await SchemaExistsAsync(username);
            if (!existingSchema)
                await sqlMigration.RunCommandAsync($"CREATE SCHEMA {username} AUTHORIZATION [GLOBAL\\{username}]");
        }

        private async Task WaitUntil(Func<bool> condition, TimeSpan maxWaitTime, TimeSpan pollInterval)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < maxWaitTime)
            {
                if(condition())
                    return;
                await Task.Delay(pollInterval);
            }
            if(condition())
                return;
            throw new TimeoutException("WaitUntil waited for maximum amount of time, but condition is still not met");
        }

        private Task<bool> SchemaExistsAsync(string username)
        {
            return sqlMigration.ExistsAsync("[sys].[schemas]", $"name = '{username}'");
        }

        private Task<bool> LoginExistsAsync(string username)
        {
            return sqlMigration.ExistsAsync("master.dbo.syslogins", $"name = 'GLOBAL\\{username}'");
        }

        private Task<bool> UserExistsAsync(string username)
        {
            return sqlMigration.ExistsAsync("[sys].[database_principals]", $"name = 'GLOBAL\\{username}'");
        }

        private async Task CreateTableIfNotExistsAsync(string schema, string tableName)
        {
            if(await TableExistsAsync(schema, tableName))
                return;
            var query = $"CREATE TABLE {tablePath} ("
                        + $"{DataApiIdColumnName} nvarchar(64) NOT NULL UNIQUE, "
                        + $"PRIMARY KEY ({DataApiIdColumnName})"
                        + ")";
            await sqlMigration.RunCommandAsync(query);
        }

        private async Task EnsureColumnsExistAsync(string databaseName, string schema, string tableName, IEnumerable<string> columnNames)
        {
            var existingColumns = await ListColumnsAsync(databaseName, schema, tableName);
            var missingColumns = columnNames.Except(existingColumns).ToList();
            if(!missingColumns.Any())
                return;
            var columnDefinitions = string.Join(",", missingColumns.Select(columnName => $"{columnName} nvarchar(100) NULL"));
            var query = $"ALTER TABLE {tablePath} ADD {columnDefinitions}";
            await sqlMigration.RunCommandAsync(query);
        }

        private async Task<List<string>> ListColumnsAsync(string databaseName, string schema, string tableName)
        {
            var query = $"SELECT COLUMN_NAME FROM {databaseName}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = '{schema}'";
            var columnNames = new List<string>();
            await sqlMigration.BuildResultAsync(
                query,
                reader =>
                {
                    var columnName = reader.GetString(0);
                    columnNames.Add(columnName);
                });
            return columnNames;
        }
    }
}
