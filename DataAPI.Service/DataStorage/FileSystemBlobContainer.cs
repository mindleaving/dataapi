using System.IO;
using System.Threading.Tasks;

namespace DataAPI.Service.DataStorage
{
    class FileSystemBlobContainer : IBlobContainer
    {
        private readonly string path;

        public FileSystemBlobContainer(string path)
        {
            this.path = path;
        }

        public Task<bool> ExistsAsync()
        {
            return Task.FromResult(Directory.Exists(path));
        }

        public Task CreateIfNotExistsAsync()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Task.CompletedTask;
        }

        public IBlob GetBlob(string id)
        {
            return new FileSystemBlob(Path.Combine(path, id));
        }
    }
}