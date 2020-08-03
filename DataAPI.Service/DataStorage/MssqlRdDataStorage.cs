using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using DataAPI.Service.Search.Sql;
using MongoDB.Bson;

namespace DataAPI.Service.DataStorage
{
    public class MssqlRdDataStorage : IRdDataStorage
    {
        private readonly string databaseName;
        private readonly QueryBuilder queryBuilder;
        private readonly SqlQueryExecutor sqlQueryExecutor;
        private readonly IIdGeneratorManager idGeneratorManager;

        public MssqlRdDataStorage(
            string id,
            string serverAddress,
            string databaseName,
            string sqlUserName,
            string sqlUserPassword,
            IIdGeneratorManager idGeneratorManager)
        {
            Id = id;
            this.databaseName = databaseName;
            this.idGeneratorManager = idGeneratorManager;
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = serverAddress,
                InitialCatalog = databaseName,
                UserID = sqlUserName,
                Password = sqlUserPassword
            }.ConnectionString;
            queryBuilder = new QueryBuilder(fieldName => fieldName.Replace('.', GenericDataContainerTraversal.PathDelimiter));
            sqlQueryExecutor = new SqlQueryExecutor(sqlConnectionString);
        }

        public string Id { get; }

        public bool IsDataTypeSupported(string dataType)
        {
            return true;
        }

        public bool IsIdGeneratorTypeSupported(IdGeneratorType idGeneratorType)
        {
            return true;
        }

        public bool IsValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return Regex.IsMatch(id, "[\x20-\x7e]{1,64}");
        }

        public async Task<List<IdReservationResult>> GetIdsAsync(string dataType, string submitter, int count)
        {
            var ids = await idGeneratorManager.GetIdsAsync(dataType, count);
            var reservationResults = new List<IdReservationResult>();
            foreach (var id in ids)
            {
                var reservationResult = await ReserveIdAsync(dataType, id, submitter);
                reservationResults.Add(reservationResult);
            }
            return reservationResults;
        }

        public async Task<IdReservationResult> ReserveIdAsync(string dataType, string id, string submitter)
        {
            var isNewCollection = await IsNewCollectionAsync(dataType);
            if (isNewCollection)
                await CreateTable(dataType);
            else if(await ExistsAsync(dataType, id))
                return IdReservationResult.Failed();
            var utcNow = DateTime.UtcNow;
            var query = $"INSERT INTO {dataType} (Id,OriginalSubmitter,CreatedTimeUtc,Submitter,SubmissionTimeUtc) "
                        + $"VALUES ('{id}','{submitter}','{utcNow:yyyy-MM-dd HH:mm:ss}','{submitter}','{utcNow:yyyy-MM-dd HH:mm:ss}')";
            try
            {
                await sqlQueryExecutor.ExecuteQueryAsync(query);
                return IdReservationResult.Success(id, isNewCollection);
            }
            catch
            {
                return IdReservationResult.Failed();
            }
        }

        public async Task<StoreResult> StoreAsync(string dataType, GenericDataContainer container, bool overwrite)
        {
            var isNewCollection = await IsNewCollectionAsync(dataType);
            if (isNewCollection)
                await CreateTable(dataType);
            var exists = await ExistsAsync(dataType, container.Id);
            if(exists && !overwrite)
                throw new DocumentAlreadyExistsException($"Object of type '{dataType}' with ID '{container.Id}' already exists");
            await EnsureColumnsExistAsync(dataType, container);
            var query = exists
                ? SqlUpdateStatement.CreateFromContainer(dataType, container)
                : SqlInsertStatement.CreateFromContainer(dataType, container);
            await sqlQueryExecutor.ExecuteQueryAsync(query);
            var modificationType = !exists
                ? DataModificationType.Created
                : DataModificationType.Replaced;
            return new StoreResult(container.Id, modificationType, isNewCollection);
        }

        private async Task<bool> IsNewCollectionAsync(string dataType)
        {
            var query = $"SELECT TABLE_NAME FROM {databaseName}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = '{dataType}'";
            var reader = await sqlQueryExecutor.ExecuteReaderAsync(query).FirstOrDefaultAsync();
            return reader == null;
        }

        public async Task<bool> ExistsAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return false;
            var query = $"SELECT TOP 1 * FROM {dataType} WHERE Id = '{id}'";
            var exists = await sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => true).FirstOrDefaultAsync(); // (mis?)uses that 'false' is default
            return exists;
        }

        public async Task<GenericDataContainer> GetFromIdAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return null;
            var query = $"SELECT TOP 1 * FROM {dataType} WHERE Id = '{id}'";
            var container = await sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => GenericDataContainerReconstructor.Reconstruct((SqlDataReader)reader)).FirstOrDefaultAsync();
            return container;
        }

        public IAsyncEnumerable<GenericDataContainer> GetManyAsync(
            string dataType,
            string whereArguments,
            string orderByArguments,
            uint? limit = null)
        {
            var query = queryBuilder.Build(
                new DataApiSqlQuery(
                    fromArguments: dataType, 
                    whereArguments: whereArguments, 
                    orderByArguments: orderByArguments, 
                    limitArguments: limit?.ToString()), 
                dataType);
            return sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => GenericDataContainerReconstructor.Reconstruct((SqlDataReader) reader));
        }

        public IAsyncEnumerable<BsonDocument> SearchAsync(DataApiSqlQuery parsedQuery, uint? maxResults = null)
        {
            var dataType = parsedQuery.FromArguments;
            var query = queryBuilder.Build(parsedQuery, dataType);
            return sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => BsonDocumentBuilder.BuildFromDelimitedColumns((SqlDataReader) reader));
        }

        public async Task<bool> DeleteDataContainerAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return false;
            var query = $"DELETE FROM {dataType} WHERE Id = '{id}'";
            var rowsAffected = await sqlQueryExecutor.ExecuteQueryAsync(query);
            return rowsAffected > 0;
        }

        public IAsyncEnumerable<string> ListCollectionNamesAsync()
        {
            var query = $"SELECT TABLE_NAME FROM {databaseName}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            return sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => reader.GetString(0));
        }

        private async Task CreateTable(string dataType)
        {
            var query = $"CREATE TABLE {dataType} ("
                            + "SqlId int IDENTITY NOT NULL, "
                            + "Id nvarchar(64) NOT NULL UNIQUE, "
                            + "OriginalSubmitter nvarchar(64) NOT NULL, "
                            + "CreatedTimeUtc datetime NOT NULL, "
                            + "Submitter nvarchar(64) NOT NULL, "
                            + "SubmissionTimeUtc datetime NOT NULL, "
                            + "PRIMARY KEY (SqlId)"
                        + "); "
                        + $"CREATE INDEX Id ON {dataType}(Id);";
            await sqlQueryExecutor.ExecuteQueryAsync(query);
        }

        private async Task EnsureColumnsExistAsync(string dataType, GenericDataContainer container)
        {
            var columnNames = GetColumnNamesFromContainer(container);
            var existingColumns = await ListColumnsAsync(dataType);
            var missingColumns = columnNames.Except(existingColumns).ToList();
            if(!missingColumns.Any())
                return;
            var columnDefinitions = string.Join(',', missingColumns.Select(columnName => $"{columnName} nvarchar(100) NULL"));
            var query = $"ALTER TABLE {dataType} ADD {columnDefinitions}";
            await sqlQueryExecutor.ExecuteQueryAsync(query);
        }

        private ValueTask<List<string>> ListColumnsAsync(string dataType)
        {
            var query = $"SELECT COLUMN_NAME FROM {databaseName}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{dataType}' AND TABLE_SCHEMA = 'dbo'";
            return sqlQueryExecutor.ExecuteReaderAsync(query).Select(reader => reader.GetString(0)).ToListAsync();
        }

        private IEnumerable<string> GetColumnNamesFromContainer(GenericDataContainer container)
        {
            var keyValuePairs = GenericDataContainerTraversal.Traverse(container);
            return keyValuePairs.Keys;
        }
    }
}
