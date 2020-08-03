using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlMigrationTools
{
    public class SqlMigration : ISqlMigration
    {
        private readonly string connectionString;

        public SqlMigration(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<bool> TableExistsAsync(
            string tableName)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();
                var commandText = @"IF EXISTS( SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table ) SELECT 1 ELSE SELECT 0";
                using (var cmd = new SqlCommand(commandText, sqlConnection))
                {
                    cmd.Parameters.Add("@table", SqlDbType.NVarChar).Value = tableName;
                    var exists = (int)await cmd.ExecuteScalarAsync();
                    return exists == 1;
                }
            }
        }

        public async Task RunCommandAsync(string command)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();
                using (var sqlCommand = new SqlCommand(command, sqlConnection))
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task BuildResultFromStoredProcedureAsync(
            string procedureName, 
            Dictionary<string, object> parameters, 
            Action<SqlDataReader> resultBuildingFunc,
            CancellationToken cancellationToken = default)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(procedureName, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                await sqlConnection.OpenAsync(cancellationToken);
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if(!reader.HasRows)
                        return;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        resultBuildingFunc(reader);
                        if(cancellationToken.IsCancellationRequested)
                            break;
                    }
                }
            }
        }

        public async Task BuildResultAsync(
            string query, 
            Action<SqlDataReader> resultBuildingFunc,
            CancellationToken cancellationToken = default)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, sqlConnection))
            {
                await sqlConnection.OpenAsync(cancellationToken);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if(!reader.HasRows)
                        return;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        resultBuildingFunc(reader);
                        if(cancellationToken.IsCancellationRequested)
                            break;
                    }
                }
            }
        }

        public async Task DeleteMatchingAsync(
            string tableName,
            string whereClause)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                // Delete existing entries
                using (var command = new SqlCommand($"DELETE FROM {tableName} WHERE {whereClause}", sqlConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task BatchUploadAsync<T>(
            IReadOnlyList<T> inputs,
            string tableName,
            IList<string> columnNames,
            Func<T, IEnumerable<IList<string>>> transformFunc)
        {
            const int MaxRowsPerQuery = 1000;

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                // Upload new
                var batchValues = new List<string>();
                foreach (var item in inputs)
                {
                    var outputs = transformFunc(item);
                    foreach (var output in outputs)
                    {
                        if(output.Count != columnNames.Count)
                            throw new Exception($"Transformation returned {output.Count} output-values, but there are {columnNames.Count} columns");
                        var safeOutput = output.Select(EscapeSqlValue);
                        batchValues.Add($"({string.Join(",", safeOutput)})");
                        if (batchValues.Count == MaxRowsPerQuery)
                        {
                            await BuildAndSubmitQuery(sqlConnection, tableName, columnNames, batchValues);
                            batchValues = new List<string>();
                        }
                    }
                }

                if (batchValues.Any())
                {
                    await BuildAndSubmitQuery(sqlConnection, tableName, columnNames, batchValues);
                }
            }
        }

        private string EscapeSqlValue(string arg)
        {
            if (arg == null)
                return "NULL";
            var escapedValue = arg;
            if (arg.StartsWith("'") && arg.EndsWith("'"))
                escapedValue = escapedValue.Substring(1, arg.Length - 2);
            escapedValue = escapedValue.Replace("'", "''");
            escapedValue = $"'{escapedValue}'";
            return escapedValue;
        }

        private static async Task BuildAndSubmitQuery(
            SqlConnection sqlConnection,
            string tableName,
            IList<string> columnNames,
            IEnumerable<string> batchValues)
        {
            var query = $"INSERT INTO {tableName} ({string.Join(",",columnNames)}) "
                        + $"VALUES {string.Join(",", batchValues)}";
            using (var command = new SqlCommand(query, sqlConnection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> ExistsAsync(string tableName, string whereClause)
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                using (var sqlCommand = new SqlCommand($"SELECT TOP 1 * FROM {tableName} WHERE {whereClause}", sqlConnection))
                {
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        return await reader.ReadAsync();
                    }
                }
            }
        }
    }
}
