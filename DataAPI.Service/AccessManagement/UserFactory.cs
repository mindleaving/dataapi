using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.Service.AccessManagement
{
    public static class UserFactory
    {
        public static User Create(RegistrationInformation registrationInformation)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(registrationInformation.Username);
            var salt = CreateSalt();
            var saltBase64 = Convert.ToBase64String(salt);
            var passwordHash = PasswordHasher.Hash(registrationInformation.Password, salt, PasswordHasher.RecommendedHashLength);
            var passwordHashBase64 = Convert.ToBase64String(passwordHash);
            return new User(
                normalizedUsername, 
                registrationInformation.FirstName,
                registrationInformation.LastName,
                registrationInformation.Email,
                saltBase64, 
                passwordHashBase64,
                new List<Role>());
        }

        private static byte[] CreateSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
