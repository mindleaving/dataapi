using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace MongoDbBackupUploader
{
    public class FileUploader
    {
        private readonly CloudBlobContainer container;

        public FileUploader(CloudStorageAccount account, string containerName)
        {
            var client = account.CreateCloudBlobClient();
            container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();
        }

        public void Upload(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            var blob = container.GetBlockBlobReference(filename);
            blob.UploadFromByteArray(new byte[] { 0x01 }, 0, 1);
            blob.SetStandardBlobTier(StandardBlobTier.Cool);
            blob.StreamWriteSizeInBytes = 100 * 1024 * 1024;
            blob.UploadFromFile(filePath);
        }
    }
}
