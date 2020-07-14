using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.Helpers;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using DataAPI.Service.Search.Sql;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public class ExistingMssqlTablesRdDataStorage : IRdDataStorage
    {
        private readonly IRdDataStorage metadataStorage;
        private readonly Dictionary<string, SqlTableSetup> tableSetups;
        private readonly QueryBuilder queryBuilder;

        public ExistingMssqlTablesRdDataStorage(
            string id,
            IRdDataStorage metadataStorage,
            IList<SqlTableSetup> tableSetups)
        {
            Id = id;
            this.metadataStorage = metadataStorage;
            this.tableSetups = tableSetups.ToDictionary(x => x.DataType, x => x);
            queryBuilder = new QueryBuilder(fieldName => fieldName.Replace("Data.",""));
        }

        public string Id { get; }

        public bool IsDataTypeSupported(string dataType)
        {
            return tableSetups.ContainsKey(dataType);
        }

        public bool IsIdGeneratorTypeSupported(IdGeneratorType idGeneratorType)
        {
            return idGeneratorType == IdGeneratorType.Integer;
        }

        public bool IsValidId(string id)
        {
            return int.TryParse(id, out _);
        }

        public async Task<List<IdReservationResult>> GetIdsAsync(string dataType, string submitter, int count)
        {
            var tableSetup = tableSetups[dataType];
            var reservationContainer = tableSetup.BuildReservationContainer(submitter);
            var reservationResults = new List<IdReservationResult>();
            for (int i = 0; i < count; i++)
            {
                IdReservationResult reservationResult;
                try
                {
                    var storeResult = await StoreAsync(dataType, reservationContainer, false);
                    reservationResult = IdReservationResult.Success(storeResult.Id, storeResult.IsNewCollection);
                }
                catch (DocumentAlreadyExistsException)
                {
                    reservationResult = IdReservationResult.Failed();
                }
                reservationResults.Add(reservationResult);
            }
            return reservationResults;
        }

        public Task<IdReservationResult> ReserveIdAsync(string dataType, string id, string submitter)
        {
            throw new NotSupportedException("Reservation of ID messes with the identity column, which is a bad idea.");
        }

        public async Task<StoreResult> StoreAsync(string dataType, GenericDataContainer container, bool overwrite)
        {
            var tableSetup = tableSetups[dataType];
            var exists = await ExistsAsync(dataType, container.Id);
            if(exists && !overwrite)
                throw new DocumentAlreadyExistsException($"Object of type '{dataType}' with ID '{container.Id}' already exists");
            if (exists)
            {
                var query = SqlUpdateStatement.CreateFromDataAndTableSetup(container.Data, tableSetup, container.Id);
                await tableSetup.QueryExecutor.ExecuteQueryAsync(query);
                await metadataStorage.StoreAsync(dataType, container, true);
                return new StoreResult(container.Id, DataModificationType.Replaced, false);
            }
            else
            {
                var query = SqlInsertStatement.CreateFromDataAndTableSetup(container.Data, tableSetup);
                var id = await tableSetup.QueryExecutor.ExecuteReaderAsync(query).Select(x => x.GetInt32(0)).FirstOrDefaultAsync();
                var idReplacedContainer = new GenericDataContainer(
                    id.ToString(),
                    container.OriginalSubmitter,
                    container.CreatedTimeUtc,
                    container.Submitter,
                    container.SubmissionTimeUtc,
                    container.ApiVersion,
                    container.Data);
                await metadataStorage.StoreAsync(dataType, idReplacedContainer, true);
                return new StoreResult(id.ToString(), DataModificationType.Created, false);
            }
        }

        public async Task<bool> ExistsAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return false;
            var tableSetup = tableSetups[dataType];
            var query = $"SELECT TOP 1 * FROM {tableSetup.TableName} WHERE {tableSetup.IdColumnName} = '{id}'";
            var exists = await tableSetup.QueryExecutor.ExecuteReaderAsync(query)
                .Select(reader => true)
                .FirstOrDefaultAsync(); // (mis?)uses that 'false' is default
            return exists;
        }

        public async Task<GenericDataContainer> GetFromIdAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return null;
            var tableSetup = tableSetups[dataType];
            var query = $"SELECT TOP 1 * FROM {tableSetup.TableName} WHERE {tableSetup.IdColumnName} = '{id}'";
            return await tableSetup.QueryExecutor.ExecuteReaderAsync(query)
                .SelectAwait(async reader => await BuildContainer((SqlDataReader)reader, tableSetup))
                .FirstOrDefaultAsync();
        }

        public async IAsyncEnumerable<GenericDataContainer> GetManyAsync(
            string dataType,
            string whereArguments,
            string orderByArguments,
            uint? limit = null)
        {
            var tableSetup = tableSetups[dataType];
            var query = queryBuilder.Build(
                new DataApiSqlQuery(
                    fromArguments: dataType, 
                    whereArguments: whereArguments, 
                    orderByArguments: orderByArguments, 
                    limitArguments: limit?.ToString()), 
                tableSetup.TableName);
            var searchResult = tableSetup.QueryExecutor.ExecuteReaderAsync(query);
            await foreach (var reader in searchResult)
            {
                yield return await BuildContainer((SqlDataReader)reader, tableSetup);
            }
        }

        public async IAsyncEnumerable<BsonDocument> SearchAsync(DataApiSqlQuery parsedQuery, uint? maxResults = null)
        {
            var dataType = parsedQuery.FromArguments;
            var tableSetup = tableSetups[dataType];
            var hasProjection = parsedQuery.SelectArguments != null && parsedQuery.SelectArguments != "*";
            var query = queryBuilder.Build(parsedQuery, tableSetup.TableName);
            var searchResults = tableSetup.QueryExecutor.ExecuteReaderAsync(query);
            await foreach (var reader in searchResults)
            {
                var container = await BuildContainer((SqlDataReader) reader, tableSetup);
                var bsonDocument = container.ToBsonDocument();
                if(!hasProjection)
                    yield return bsonDocument;
                else
                    yield return MapContainerUsingSelect(container, parsedQuery);
            }
        }

        private BsonDocument MapContainerUsingSelect(GenericDataContainer container, DataApiSqlQuery parsedQuery)
        {
            var containerJObject = JObject.FromObject(container);
            var dataJObject = JObject.Parse(DataEncoder.DecodeToJson(container.Data));
            var fieldMappings = parsedQuery.SelectArguments.Split(',').Select(x => x.Trim());
            var bsonDocument = new BsonDocument();
            foreach (var fieldMapping in fieldMappings)
            {
                var match = Regex.Match(fieldMapping, "^(?<Path>[^\\s]+)(?<Map>\\s+AS\\s+(?<NewName>[^\\s]+))?", RegexOptions.IgnoreCase);
                if(!match.Success)
                    throw new FormatException($"Invalid SELECT statement '{fieldMapping}'");
                var fieldPath = match.Groups["Path"].Value;
                string newName;
                var hasMap = match.Groups["Map"].Success;
                if (hasMap)
                    newName = match.Groups["NewName"].Value;
                else
                    newName = fieldPath.Replace('.', '_');
                JToken jToken;
                if (fieldPath.StartsWith("Data."))
                {
                    var token = hasMap ? newName : fieldPath.Substring("Data.".Length);
                    jToken = dataJObject.SelectToken(token);
                }
                else if (fieldPath == "Data")
                {
                    jToken = dataJObject;
                }
                else
                {
                    jToken = containerJObject.SelectToken(fieldPath);
                }
                BsonValue bsonValue;
                if (jToken == null)
                    bsonValue = BsonNull.Value;
                else if (jToken is JValue jValue)
                    bsonValue = BsonValue.Create(jValue.Value);
                else
                    bsonValue = BsonDocument.Parse(jToken.ToString());
                bsonDocument.Add(new BsonElement(newName, bsonValue));
            }
            return bsonDocument;
        }

        public async Task<bool> DeleteDataContainerAsync(string dataType, string id)
        {
            if (!IsValidId(id))
                return false;
            var tableSetup = tableSetups[dataType];
            var query = $"DELETE FROM {tableSetup.TableName} WHERE {tableSetup.IdColumnName} = '{id}'";
            var rowsAffected = await tableSetup.QueryExecutor.ExecuteQueryAsync(query);
            return rowsAffected > 0;
        }

        public IAsyncEnumerable<string> ListCollectionNamesAsync()
        {
            return AsyncEnumerableBuilder.FromArray(tableSetups.Keys.ToList());
        }

        private async Task<GenericDataContainer> BuildContainer(SqlDataReader reader, SqlTableSetup tableSetup)
        {
            var id = (int)reader[tableSetup.IdColumnName];
            var metadata = await metadataStorage.GetFromIdAsync(tableSetup.DataType, id.ToString());
            if(metadata == null)
            {
                var utcNow = DateTime.UtcNow;
                metadata = new GenericDataContainer(
                    id.ToString(),
                    "unknown",
                    utcNow,
                    "unknown",
                    utcNow,
                    ApiVersion.Current,
                    new BsonDocument());
            }

            var data = BsonDocumentBuilder.BuildFromDelimitedColumns(reader, tableSetup);
            return new GenericDataContainer(
                id.ToString(),
                metadata.OriginalSubmitter,
                metadata.CreatedTimeUtc,
                metadata.Submitter,
                metadata.SubmissionTimeUtc,
                metadata.ApiVersion,
                data);
        }
    }
}
