using System;
using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace DataAPI.Service.AccessManagement
{
    public class User
    {
        [JsonConstructor]
        public User(
            string userName, 
            string firstName,
            string lastName,
            string email,
            string salt, 
            string passwordHash,
            List<Role> roles)
        {
            if(!IsValidUsername(userName))
                throw new ArgumentException($"Invalid username '{userName}'");
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Salt = salt ?? throw new ArgumentNullException(nameof(salt));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Roles = roles ?? new List<Role>();
        }

        [BsonId]
        public string Id => UserName;
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Salt { get; private set; }
        public string PasswordHash { get; set; }
        public List<Role> Roles { get; private set; }

        private static bool IsValidUsername(string userName)
        {
            return !string.IsNullOrEmpty(userName);
            //return Regex.IsMatch(userName, "[a-zA-Z0-9_-.]+");
        }
    }
}