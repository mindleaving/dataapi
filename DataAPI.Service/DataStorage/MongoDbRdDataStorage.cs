using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using DataAPI.Service.Search.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DataAPI.Service.DataStorage
{
    public class MongoDbRdDataStorage : IRdDataStorage
    {
        private readonly RdDataMongoClient rdDataClient;
        private readonly IIdGeneratorManager idGeneratorManager;

        public MongoDbRdDataStorage(
            string id, 
            RdDataMongoClient rdDataClient,
            IIdGeneratorManager idGeneratorManager)
        {
            Id = id;
            this.rdDataClient = rdDataClient;
            this.idGeneratorManager = idGeneratorManager;
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
            var collection = rdDataClient.DataDatabase.GetCollection<GenericDataContainer>(dataType);
            try
            {
                var utcNow = DateTime.UtcNow;
                await collection.InsertOneAsync(new GenericDataContainer(id, submitter, utcNow, submitter, utcNow, ApiVersion.Current, new BsonDocument()));
                return IdReservationResult.Success(id, isNewCollection);
            }
            catch (MongoWriteException writeException)
            {
                if(writeException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    return IdReservationResult.Failed();
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string dataType, string id)
        {
            return await GetFromIdAsync(dataType, id) != null;
        }

        public async Task<StoreResult> StoreAsync(string dataType, GenericDataContainer container, bool overwrite)
        {
            var isNewCollection = await IsNewCollectionAsync(dataType);
            var collection = rdDataClient.DataDatabase.GetCollection<GenericDataContainer>(dataType);
            var id = container.Id;
            if(overwrite)
            {
                var replaceResult = await collection.ReplaceOneAsync(x => x.Id == id, container, new ReplaceOptions { IsUpsert = true });
                var dataModificationType = replaceResult.MatchedCount == 0 ? DataModificationType.Created : DataModificationType.Replaced;
                return new StoreResult(id, dataModificationType, isNewCollection);
            }

            try
            {
                await collection.InsertOneAsync(container);
                return new StoreResult(id, DataModificationType.Created, isNewCollection);
            }
            catch (MongoWriteException writeException)
            {
                if(writeException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    throw new DocumentAlreadyExistsException($"Object of type '{dataType}' with ID '{id}' already exists");
                throw;
            }
        }

        public Task<GenericDataContainer> GetFromIdAsync(string dataType, string id)
        {
            var collection = rdDataClient.DataDatabase.GetCollection<GenericDataContainer>(dataType);
            return collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async IAsyncEnumerable<GenericDataContainer> GetManyAsync(
            string dataType,
            string whereArguments,
            string orderByArguments,
            uint? limit = null)
        {
            if(whereArguments != null && (whereArguments.Contains("{") || whereArguments.Contains("}")))
                throw new FormatException(); // SECURITY NOTE: This is crucial in order to prevent SQL-injection like attacks (only with MongoDB-syntax instead of SQL)
            var collection = rdDataClient.DataDatabase.GetCollection<BsonDocument>(dataType);

            var parsedQuery = new DataApiSqlQuery(
                fromArguments: dataType,
                whereArguments: whereArguments,
                orderByArguments: orderByArguments,
                limitArguments: limit >= 0 ? limit.ToString() : null);
            var pipeline = AggregatePipelineBuilder.Build(parsedQuery, limit);
            using var cursor = await collection.AggregateAsync(pipeline);
            while (await cursor.MoveNextAsync())
            {
                foreach (var result in cursor.Current)
                {
                    yield return BsonSerializer.Deserialize<GenericDataContainer>(result);
                }
            }
        }

        public async IAsyncEnumerable<BsonDocument> SearchAsync(DataApiSqlQuery parsedQuery, uint? maxResults = null)
        {
            var pipeline = AggregatePipelineBuilder.Build(parsedQuery, maxResults);
            var collection = rdDataClient.DataDatabase.GetCollection<BsonDocument>(parsedQuery.FromArguments);
            using var cursor = await collection.AggregateAsync(pipeline);
            while (await cursor.MoveNextAsync())
            {
                foreach (var result in cursor.Current)
                {
                    yield return result;
                }
            }
        }

        public async Task<bool> DeleteDataContainerAsync(string dataType, string id)
        {
            var collection = rdDataClient.DataDatabase.GetCollection<GenericDataContainer>(dataType);
            var deletionResult = await collection.DeleteOneAsync(x => x.Id == id);
            return deletionResult.IsAcknowledged && deletionResult.DeletedCount == 1;
        }

        public async IAsyncEnumerable<string> ListCollectionNamesAsync()
        {
            var cursor = await rdDataClient.DataDatabase.ListCollectionNamesAsync();
            while (await cursor.MoveNextAsync())
            {
                foreach (var collectionName in cursor.Current)
                {
                    yield return collectionName;
                }
            }
            cursor.Dispose();
        }

        private async Task<bool> IsNewCollectionAsync(string dataType)
        {
            var exists = await rdDataClient.DataDatabase.ListCollectionNames(
                new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", dataType)
                }).AnyAsync();
            return !exists;
        }
    }
}
