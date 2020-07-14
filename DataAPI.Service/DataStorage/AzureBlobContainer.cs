using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace DataAPI.Service.DataStorage
{
    public class AzureBlobContainer : IBlobContainer
    {
        private readonly CloudBlobContainer container;

        public AzureBlobContainer(CloudBlobContainer container)
        {
            this.container = container;
        }

        public Task<bool> ExistsAsync()
        {
            return container.ExistsAsync();
        }

        public Task CreateIfNotExistsAsync()
        {
            return container.CreateIfNotExistsAsync();
        }

        public IBlob GetBlob(string id)
        {
            return new AzureBlob(container.GetBlockBlobReference(id));
        }
    }
}