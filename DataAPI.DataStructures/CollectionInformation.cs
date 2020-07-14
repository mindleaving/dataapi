using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json;

namespace DataAPI.DataStructures
{
    public class CollectionInformation
    {
        [JsonConstructor]
        public CollectionInformation(
            string collectionName,
            string displayName,
            string description,
            bool isProtected,
            bool nonAdminUsersCanOverwriteData,
            bool isHidden,
            IdGeneratorType idGeneratorType,
            string username, 
            List<Role> userRoles,
            List<ValidatorDefinition> validatorDefinitions)
        {
            CollectionName = collectionName;
            DisplayName = displayName;
            Description = description;
            IsProtected = isProtected;
            Username = username;
            NonAdminUsersCanOverwriteData = nonAdminUsersCanOverwriteData;
            IsHidden = isHidden;
            UserRoles = userRoles;
            ValidatorDefinitions = validatorDefinitions;
            IdGeneratorType = idGeneratorType;
        }

        public string CollectionName { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public bool IsProtected { get; }
        public bool NonAdminUsersCanOverwriteData { get; }
        public bool IsHidden { get; }
        public IdGeneratorType IdGeneratorType { get; }
        public string Username { get; }
        public List<Role> UserRoles { get; }
        public List<ValidatorDefinition> ValidatorDefinitions { get; }
    }

    public enum IdGeneratorType
    {
        Guid = 0,
        Integer = 1
    }
}
