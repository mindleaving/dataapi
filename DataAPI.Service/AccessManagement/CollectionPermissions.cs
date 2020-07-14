using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace DataAPI.Service.AccessManagement
{
    public class CollectionPermissions
    {
        [JsonConstructor]
        public CollectionPermissions(
            string collectionName,
            string username,
            List<Role> roles)
        {
            CollectionName = collectionName;
            Username = username;
            Roles = roles;
        }

        [BsonId]
        public string Id => GetId(CollectionName, Username);
        public string CollectionName { get; private set; }
        public string Username { get; private set; }
        public List<Role> Roles { get; private set; }

        public static string GetId(string collectionName, string username)
        {
            return $"{collectionName}_{username}";
        }
    }
}
