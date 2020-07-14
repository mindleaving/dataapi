using System;
using System.IO;
using System.Threading.Tasks;

namespace DataAPI.Service.DataStorage
{
    public class FileSystemBlob : IBlob
    {
        private readonly string path;

        public FileSystemBlob(string path)
        {
            this.path = path;
        }

        public DateTime CreatedTimestampUtc => new FileInfo(path).CreationTimeUtc;
        public DateTime LastModifiedTimestampUtc => new FileInfo(path).LastWriteTimeUtc;

        public Task<bool> ExistsAsync()
        {
            return Task.FromResult(File.Exists(path));
        }

        public Stream GetStream()
        {
            return File.OpenRead(path);
        }

        public async Task WriteAsync(byte[] bytes)
        {
            await File.WriteAllBytesAsync(path, bytes);
        }

        public async Task WriteAsync(Stream stream)
        {
            await using var fileStream = File.OpenWrite(path);
            await stream.CopyToAsync(fileStream);
        }

        public Task DeleteAsync()
        {
            File.Delete(path);
            return Task.CompletedTask;
        }
    }
}