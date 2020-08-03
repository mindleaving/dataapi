using DataAPI.DataStructures;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.AccessManagement
{
    public class CollectionMetadata
    {
        [JsonConstructor]
        public CollectionMetadata(string collectionName)
        {
            CollectionName = collectionName;
        }

        [BsonId]
        public string CollectionName { get; private set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsProtected { get; set; }
        public bool NonAdminUsersCanOverwriteData { get; set; }
        public bool IsHidden { get; set; }
        public IdGeneratorType IdGeneratorType { get; set; }
    }
}
