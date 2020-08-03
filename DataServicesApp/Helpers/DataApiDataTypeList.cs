using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;

namespace DataServicesApp.Helpers
{
    public class DataApiDataTypeList : IDataTypeList
    {
        private readonly IDataApiClient dataApiClient;

        public DataApiDataTypeList(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
        }

        public List<CollectionInformation> GetCollections()
        {
            return Task.Run(async () => await dataApiClient.ListCollectionsAsync()).Result;
        }
    }
}