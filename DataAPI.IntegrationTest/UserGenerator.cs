using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Commons;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.IntegrationTest
{
    internal static class UserGenerator
    {
        public static string GenerateUsername()
        {
            var usernameLength = 16;
            var characters = Enumerable.Range(0, 26).Select(offset => (char)('A' + offset))
                .Concat(Enumerable.Range(0, 26).Select(offset => (char)('a' + offset)))
                .Concat(Enumerable.Range(0, 10).Select(offset => (char)('0' + offset)))
                .ToList();
            return "Test_" + GenerateRandomCharacterSequence(usernameLength, characters);
        }

        /// <summary>
        /// Generates a cryptographically strong password
        /// of length between 16 and 32 with all visible ASCII-characters
        ///
        /// SECURITY NOTE:
        /// Even though this is a unit test, it's important that the login is secure,
        /// because an attacker can otherwise try to exploit the unit test login for accessing the API
        /// </summary>
        public static string GeneratePassword()
        {
            var characters = Enumerable.Range(33, 126 - 33 + 1).Select(idx => (char) idx).ToList();
            var password = string.Empty;
            using (var cryptoRng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[1];
                cryptoRng.GetBytes(bytes);
                var length = 16 + ((int) bytes[0]).Modulus(16); // It's important that the modulus is a divisor of 256 such that all lengths are equally likely
                while (password.Length < length)
                {
                    cryptoRng.GetBytes(bytes);
                    if (bytes[0] < characters.Count) // Skip if number not in range.
                        // Don't use modulus to achieve an index inside the character set length,
                        // because that would make some characters more likely than others.
                    {
                        password += characters[bytes[0]];
                    }
                }
            }
            return password;
        }

        private static string GenerateRandomCharacterSequence(int length, IList<char> characterSet)
        {
            return new string(Enumerable.Range(0, length)
                .Select(_ => characterSet[StaticRandom.Rng.Next(characterSet.Count)])
                .ToArray());
        }

        public static AuthenticationResult RegisterAndLoginUserWithoutRoles(out IDataApiClient dataApiClient)
        {
            var username = GenerateUsername();
            var password = GeneratePassword();
            var email = $"{username}@example.org";
            var firstName = "Jamie";
            var lastName = "Doe";
            dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            dataApiClient.Register(username, firstName, lastName, password, email);
            return dataApiClient.Login(username, password);
        }

        public static AuthenticationResult RegisterAndLoginUserWithRole(Role role, IDataApiClient adminDataApiClient, out IDataApiClient dataApiClient)
        {
            var username = GenerateUsername();
            var password = GeneratePassword();
            var email = $"{username}@example.org";
            var firstName = "Jamie";
            var lastName = "Doe";
            dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            dataApiClient.Register(username, firstName, lastName, password, email);
            adminDataApiClient.AddGlobalRoleToUser(username, role);
            return dataApiClient.Login(username, password);
        }

        public static void DeleteUser(IDataApiClient dataApiClient)
        {
            dataApiClient.DeleteUser(dataApiClient.LoggedInUsername);
        }
    }
}