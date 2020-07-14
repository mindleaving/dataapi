namespace MongoDbBackupUploader
{
    public class Config
    {
        public string BackupDirectory { get; set; }
        public string MongoDbUsername { get; set; }
        public string MongoDbPasswordEnvironmentVariable { get; set; }
        public string AzureBlobUsername { get; set; }
        public string AzureBlobKeyEnvironmentVariable { get; set; }
        public string ContainerName { get; set; }
    }
}