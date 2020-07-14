using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace DataAPI.Service.DataStorage
{
    public class AzureBlob : IBlob
    {
        private readonly CloudBlockBlob cloudBlob;

        public AzureBlob(CloudBlockBlob cloudBlob)
        {
            this.cloudBlob = cloudBlob;
        }

        public DateTime CreatedTimestampUtc => cloudBlob.Properties.Created?.UtcDateTime ?? DateTime.UtcNow;
        public DateTime LastModifiedTimestampUtc => cloudBlob.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow;

        public async Task<bool> ExistsAsync()
        {
            return await cloudBlob.ExistsAsync();
        }

        public Stream GetStream()
        {
            return cloudBlob.OpenRead();
        }

        public async Task WriteAsync(byte[] bytes)
        {
            await cloudBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }

        public async Task WriteAsync(Stream stream)
        {
            await using var blobStream = await cloudBlob.OpenWriteAsync();
            await stream.CopyToAsync(blobStream);
        }

        public Task DeleteAsync()
        {
            return cloudBlob.DeleteAsync();
        }
    }
}