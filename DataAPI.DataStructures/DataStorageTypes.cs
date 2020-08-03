namespace DataAPI.DataStructures
{
    public static class DataStorageTypes
    {
        public const string MongoDB = nameof(MongoDB);
        public const string FileSystem = nameof(FileSystem);
        public const string AzureBlobStorage = nameof(AzureBlobStorage);
        public const string ExistingSQL = nameof(ExistingSQL);
        public const string GenericSQL = nameof(GenericSQL);
    }
}
