using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.DataStorage;
using MongoDB.Driver;

namespace DataAPI.Service.IdGeneration
{
    public class IdGeneratorManager : IIdGeneratorManager
    {
        private readonly IMongoCollection<CollectionMetadata> collectionMetadataCollection;
        private readonly Dictionary<IdGeneratorType, IIdGenerator> idGenerators;

        public IdGeneratorManager(RdDataMongoClient rdDataMongoClient)
        {
            collectionMetadataCollection = rdDataMongoClient.BackendDatabase.GetCollection<CollectionMetadata>(nameof(CollectionMetadata));
            idGenerators = new Dictionary<IdGeneratorType, IIdGenerator>
            {
                { IdGeneratorType.Guid, new GuidIdGenerator() },
                { IdGeneratorType.Integer, new IntegerIdGenerator(rdDataMongoClient) }
            };
        }

        public async Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default)
        {
            var idGenerator = await GetIdGeneratorForDataTypeAsync(dataType);
            return await idGenerator.GetIdsAsync(dataType, count, cancellationToken);
        }

        public async Task<IIdGenerator> GetIdGeneratorForDataTypeAsync(string dataType)
        {
            var collectionMetadata = await collectionMetadataCollection.Find(x => x.CollectionName == dataType).FirstOrDefaultAsync();
            var idGeneratorType = collectionMetadata?.IdGeneratorType ?? Conventions.DefaultIdGenerator;
            return idGenerators[idGeneratorType];
        }
    }
}
