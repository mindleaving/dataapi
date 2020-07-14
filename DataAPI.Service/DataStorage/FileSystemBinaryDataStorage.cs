using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataAPI.Service.DataStorage
{
    public class FileSystemBinaryDataStorage : IBinaryDataStorage
    {
        private readonly string storageDirectory;

        public FileSystemBinaryDataStorage(string storageDirectory)
        {
            if(!Directory.Exists(storageDirectory))
                throw new DirectoryNotFoundException($"Storage directory '{storageDirectory}' doesn't exist. Cannot create {nameof(FileSystemBinaryDataStorage)}");
            TestPermissions(storageDirectory);
            this.storageDirectory = storageDirectory;
        }

        private static void TestPermissions(string storageDirectory)
        {
            Directory.GetDirectories(storageDirectory);
            var testDirectory = Path.Combine(storageDirectory, Guid.NewGuid().ToString());
            while (Directory.Exists(testDirectory))
            {
                testDirectory = Path.Combine(storageDirectory, Guid.NewGuid().ToString());
            }
            Directory.CreateDirectory(testDirectory);
            Directory.Delete(testDirectory);
        }

        public IEnumerable<string> ListContainers()
        {
            return Directory.GetDirectories(storageDirectory)
                .Select(directory => new DirectoryInfo(directory).Name);
        }

        public IBlobContainer GetContainer(string dataType)
        {
            return new FileSystemBlobContainer(Path.Combine(storageDirectory, dataType));
        }

        public IBlob GetBlob(string dataType, string id)
        {
            var container = GetContainer(dataType);
            return container.GetBlob(id);
        }
    }
}
