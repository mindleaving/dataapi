using System.Collections.Generic;
using System.Linq;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace DataAPI.Service.DataStorage
{
    public class AzureBinaryDataStorage : IBinaryDataStorage
    {
        private static readonly Dictionary<string, string> CollectionNameMap = new Dictionary<string, string>
        {
            { "image", nameof(Image) },
            { "datablob", nameof(DataBlob) }
        };

        private readonly CloudBlobClient client;

        public AzureBinaryDataStorage(CloudStorageAccount account)
        {
            client = account.CreateCloudBlobClient();
        }

        public IEnumerable<string> ListContainers()
        {
            return client.ListContainers()
                .Select(x => CollectionNameMap.ContainsKey(x.Name) ? CollectionNameMap[x.Name] : x.Name);
        }

        public IBlobContainer GetContainer(string dataType)
        {
            var container = client.GetContainerReference(dataType.ToLowerInvariant());
            return new AzureBlobContainer(container);
        }

        public IBlob GetBlob(string dataType, string id)
        {
            var container = GetContainer(dataType);
            return container.GetBlob(id);
        }
    }
}