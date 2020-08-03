using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.UserManagement;
using MongoDB.Driver;

namespace DataAPI.Service.AccessManagement
{
    public class AuthenticationModule
    {
        private readonly IMongoCollection<User> userCollection;
        private readonly IMongoCollection<CollectionPermissions> collectionPermissionsCollection;
        private readonly ISecurityTokenBuilder securityTokenBuilder;

        public AuthenticationModule(AccessControlMongoClient mongoDbClient, ISecurityTokenBuilder securityTokenBuilder)
        {
            this.securityTokenBuilder = securityTokenBuilder;
            var authenticationDatabase = mongoDbClient.Database;
            userCollection = authenticationDatabase.GetCollection<User>(nameof(User));
            // Consider referencing authroization module, which then can handle deletion of collection permissions when a user is deleted
            collectionPermissionsCollection = authenticationDatabase.GetCollection<CollectionPermissions>(nameof(CollectionPermissions));
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await userCollection.InsertOneAsync(user);
                return true;
            }
            catch (MongoDuplicateKeyException)
            {
                return false;
            }
        }

        private async Task<bool> UpdateUserAsync(string username, UpdateDefinition<User> updateDefinition)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            var updateResult = await userCollection.UpdateOneAsync(x => x.UserName == normalizedUsername, updateDefinition);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount == 1;
        }

        public async Task<bool> ChangePasswordAsync(string username, string password)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            var matchingUser = await userCollection.Find(x => x.UserName == normalizedUsername).FirstOrDefaultAsync();
            if (matchingUser == null)
                return false;
            var saltBytes = Convert.FromBase64String(matchingUser.Salt);
            var passwordHash = PasswordHasher.Hash(password, saltBytes, PasswordHasher.RecommendedHashLength);
            var passwordBase64 = Convert.ToBase64String(passwordHash);

            return await UpdateUserAsync(normalizedUsername, Builders<User>.Update.Set(x => x.PasswordHash, passwordBase64));
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            var collectionPermissionDeletionTask = collectionPermissionsCollection.DeleteManyAsync(x => x.Username == normalizedUsername);
            var userDeletionTask = userCollection.DeleteOneAsync(x => x.UserName == normalizedUsername);
            await Task.WhenAll(collectionPermissionDeletionTask, userDeletionTask);
            return userDeletionTask.Result.DeletedCount == 1;
        }

        public async Task<User> FindUserAsync(string username)
        {
            var normalizedUsername = UsernameNormalizer.Normalize(username);
            return await userCollection.Find(x => x.UserName == normalizedUsername).FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(string username)
        {
            var existingUser = await FindUserAsync(username);
            return existingUser != null;
        }

        public IAsyncEnumerable<UserProfile> GetAllUserProfilesAsync()
        {
            return userCollection.Find(x => true)
                .ToAsyncEnumerable()
                .Select(user => new UserProfile(user.UserName, user.FirstName, user.LastName, user.Email));
        }

        public async Task<List<Role>> GetGlobalRolesForUserAsync(string username)
        {
            var matchingUser = await FindUserAsync(username);
            if(matchingUser == null)
                throw new KeyNotFoundException($"User '{username}' doesn't exist");
            return matchingUser.Roles;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(LoginInformation loginInformation)
        {
            var existingUser = await FindUserAsync(loginInformation.Username);
            if(existingUser == null)
                return AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);
            if(string.IsNullOrEmpty(loginInformation.Password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            var salt = Convert.FromBase64String(existingUser.Salt);
            var storedPasswordHash = Convert.FromBase64String(existingUser.PasswordHash);
            var providedPasswordHash = PasswordHasher.Hash(loginInformation.Password, salt, 8 * storedPasswordHash.Length);
            var isMatch = HashComparer.Compare(providedPasswordHash, storedPasswordHash);
            if (!isMatch)
            {
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            }

            return BuildSecurityTokenForUser(existingUser);
        }

        public AuthenticationResult BuildSecurityTokenForUser(User user)
        {
            var token = securityTokenBuilder.BuildForUser(user);
            return AuthenticationResult.Success(user.UserName, token);
        }
    }
}
