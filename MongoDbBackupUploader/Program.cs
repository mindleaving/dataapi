using System;
using System.IO;
using System.Linq;
using Commons.Misc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Newtonsoft.Json;

namespace MongoDbBackupUploader
{
    public static class Program
    {
        public static void Main()
        {
            var config = ReadConfig("appsettings.json");
            var mongoDbPassword = Secrets.Get(config.MongoDbPasswordEnvironmentVariable);
            var azureBlobPassword = Secrets.Get(config.AzureBlobKeyEnvironmentVariable);

            Console.Write("Building backup...");
            var backupRunner = new BackupRunner(config.BackupDirectory, config.MongoDbUsername, mongoDbPassword);
            var backupFilePath = backupRunner.Run();
            Console.WriteLine("DONE");

            Console.Write("Uploading to Azure Blob Storage...");
            var azureBlobAccount = new CloudStorageAccount(
                new StorageCredentials(config.AzureBlobUsername, azureBlobPassword), 
                useHttps: true);
            var backupUploader = new FileUploader(azureBlobAccount, config.ContainerName);
            backupUploader.Upload(backupFilePath);
            Console.WriteLine("DONE");

            Console.Write("Deleteing old local backups...");
            DeleteExistingLocalBackups(backupFilePath);
            Console.WriteLine("DONE");
        }

        private static Config ReadConfig(string configFilePath)
        {
            var json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }

        private static void DeleteExistingLocalBackups(string backupFilePath)
        {
            var backupDirectory = Path.GetDirectoryName(backupFilePath);
            var otherZipFiles = Directory
                .EnumerateFiles(backupDirectory, NamingConventions.BackupFilePrefix + "*")
                .Except(new[] {backupFilePath});
            foreach (var zipFile in otherZipFiles)
            {
                File.Delete(zipFile);
            }
        }
    }
}
