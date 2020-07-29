using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Driver;

namespace DataAPI.Service.DataRouting
{
    public class DataRouter : IDataRouter
    {
        private readonly IMongoCollection<DataRedirection> dataRedirectionCollection;
        private readonly Dictionary<string, IRdDataStorage> dataStorages;
        private readonly string fallbackDataSourceSystemId;

        public DataRouter(
            RdDataMongoClient rdDataMongoClient, 
            string fallbackDataSourceSystemId, 
            List<IRdDataStorage> dataStorages)
        {
            dataRedirectionCollection = rdDataMongoClient.BackendDatabase.GetCollection<DataRedirection>(nameof(DataRedirection));
            this.fallbackDataSourceSystemId = fallbackDataSourceSystemId;
            this.dataStorages = dataStorages.ToDictionary(x => x.Id, x => x);
        }

        public bool IsAvailable(string dataSourceSystemId)
        {
            return dataStorages.ContainsKey(dataSourceSystemId);
        }

        public async Task<IRdDataStorage> GetSourceSystemAsync(string dataType)
        {
            var dataRediction = await dataRedirectionCollection.Find(x => x.DataType == dataType).FirstOrDefaultAsync();
            if (dataRediction == null)
                return dataStorages[fallbackDataSourceSystemId];
            var sourceSystem = dataRediction.SourceSystemId;
            if (!dataStorages.ContainsKey(sourceSystem))
            {
                throw new KeyNotFoundException($"Data of type '{dataType}' can't be stored. Please contact Inno-IT to clearify why this is.");
            }
            return dataStorages[sourceSystem];
        }

        public async Task SetRedirectionAsync(DataRedirection dataRedirection)
        {
            await dataRedirectionCollection.ReplaceOneAsync(
                x => x.Id == dataRedirection.Id, 
                dataRedirection, 
                new ReplaceOptions { IsUpsert = true});
        }

        public async IAsyncEnumerable<string> ListCollectionNamesAsync()
        {
            var knownCollections = new HashSet<string>(); // Used for ensuring that a distinct list of collections is returned
            foreach (var rdDataStorage in dataStorages.Values)
            {
                await foreach (var collectionName in rdDataStorage.ListCollectionNamesAsync())
                {
                    if(knownCollections.Contains(collectionName))
                        continue;
                    yield return collectionName;
                    knownCollections.Add(collectionName);
                }
            }
        }

        public bool IsDataTypeSupported(string dataType, string dataSourceSystem)
        {
            if (!dataStorages.ContainsKey(dataSourceSystem))
                return false;
            return dataStorages[dataSourceSystem].IsDataTypeSupported(dataType);
        }
    }
}
