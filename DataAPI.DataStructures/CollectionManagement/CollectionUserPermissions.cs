using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.CollectionManagement
{
    public class CollectionUserPermissions
    {
        [JsonConstructor]
        public CollectionUserPermissions(
            string collectionName,
            string username,
            List<Role> roles)
        {
            CollectionName = collectionName;
            Username = username;
            Roles = roles;
        }

        public string CollectionName { get; }
        public string Username { get; }
        public List<Role> Roles { get; }
    }
}
