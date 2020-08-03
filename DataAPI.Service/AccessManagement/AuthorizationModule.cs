using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.UserManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.DataStorage;
using MongoDB.Driver;

namespace DataAPI.Service.AccessManagement
{
    public class AuthorizationModule
    {
        private readonly IMongoCollection<User> userCollection;
        private readonly IMongoCollection<CollectionPermissions> collectionPermissionsCollection;
        private readonly IMongoCollection<CollectionMetadata> collectionMetadataCollection;

        public AuthorizationModule(
            AccessControlMongoClient accessControlMongoClient, 
            RdDataMongoClient rdDataMongoClient)
        {
            var authenticationDatabase = accessControlMongoClient.Database;
            userCollection = authenticationDatabase.GetCollection<User>(nameof(User));
            collectionPermissionsCollection = rdDataMongoClient.BackendDatabase.GetCollection<CollectionPermissions>(nameof(CollectionPermissions));
            collectionMetadataCollection = rdDataMongoClient.BackendDatabase.GetCollection<CollectionMetadata>(nameof(CollectionMetadata));

        }

        public async Task<bool> AddGlobalRoleToUser(string username, Role role)
        {
            var matchingUser = await userCollection.Find(x => x.UserName == username).FirstOrDefaultAsync();
            if (matchingUser == null)
                return false;
            if (matchingUser.Roles.Contains(role))
                return true;
            return await UpdateUser(username, Builders<User>.Update.AddToSet(x => x.Roles, role));
        }

        public async Task<bool> AddCollectionRoleToUser(string username, Role role, string collectionName)
        {
            var permissionId = CollectionPermissions.GetId(collectionName, username);
            var matchingCollectionPermission = await collectionPermissionsCollection.Find(x => x.Id == permissionId).FirstOrDefaultAsync();
            if (matchingCollectionPermission == null)
            {
                var collectionPermissions = new CollectionPermissions(collectionName, username, new List<Role> { role });
                await collectionPermissionsCollection.InsertOneAsync(collectionPermissions);
                return true;
            }
            if (matchingCollectionPermission.Roles.Contains(role))
                return true;
            return await UpdateCollectionPermission(matchingCollectionPermission.Id, Builders<CollectionPermissions>.Update.AddToSet(x => x.Roles, role));
        }

        public async Task<bool> SetGlobalRolesForUser(string username, List<Role> roles)
        {
            var matchingUser = await userCollection.Find(x => x.UserName == username).FirstOrDefaultAsync();
            if (matchingUser == null)
                return false;
            if (matchingUser.Roles.Equivalent(roles))
                return true;
            return await UpdateUser(username, Builders<User>.Update.Set(x => x.Roles, roles));
        }

        public async Task<bool> SetCollectionRolesForUser(string username, List<Role> roles, string collectionName)
        {
            var permissionId = CollectionPermissions.GetId(collectionName, username);
            var matchingCollectionPermission = await collectionPermissionsCollection.Find(x => x.Id == permissionId).FirstOrDefaultAsync();
            if (matchingCollectionPermission == null)
            {
                if (!roles.Any())
                    return true; // Nothing to do. Empty role list clears all permissions
                var collectionPermissions = new CollectionPermissions(collectionName, username, roles);
                await collectionPermissionsCollection.InsertOneAsync(collectionPermissions);
                return true;
            }

            if (matchingCollectionPermission.Roles.Equivalent(roles))
                return true;
            return await UpdateCollectionPermission(matchingCollectionPermission.Id, Builders<CollectionPermissions>.Update.Set(x => x.Roles, roles));
        }

        public async Task<bool> RemoveGlobalRoleFromUser(string username, Role role)
        {
            var matchingUser = await userCollection.Find(x => x.UserName == username).FirstOrDefaultAsync();
            if (matchingUser == null)
                return false;
            if (!matchingUser.Roles.Contains(role))
                return true;
            return await UpdateUser(username, Builders<User>.Update.Pull(x => x.Roles, role));
        }

        public async Task<bool> RemoveCollectionRoleFromUser(string username, Role role, string collectionName)
        {
            var permissionId = CollectionPermissions.GetId(collectionName, username);
            var matchingCollectionPermission = await collectionPermissionsCollection.Find(x => x.Id == permissionId).FirstOrDefaultAsync();
            if (matchingCollectionPermission == null)
                return true;
            if (!matchingCollectionPermission.Roles.Contains(role))
                return true;
            return await UpdateCollectionPermission(matchingCollectionPermission.Id, Builders<CollectionPermissions>.Update.Pull(x => x.Roles, role));
        }

        private async Task<bool> UpdateUser(string username, UpdateDefinition<User> updateDefinition)
        {
            var updateResult = await userCollection.UpdateOneAsync(x => x.UserName == username, updateDefinition);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount == 1;
        }

        private async Task<bool> UpdateCollectionPermission(string permissionId, UpdateDefinition<CollectionPermissions> updateDefinition)
        {
            var updateResult = await collectionPermissionsCollection.UpdateOneAsync(x => x.Id == permissionId, updateDefinition);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount == 1;
        }

        public async Task<AuthorizationResult> AuthorizeAsync(IResourceDescription resourceDescription, string username)
        {
            var user = await userCollection.Find(x => x.UserName == username).FirstOrDefaultAsync();
            if(user == null)
                return AuthorizationResult.Denied();
            return await AuthorizeAsync(resourceDescription, user);
        }

        private async Task<AuthorizationResult> AuthorizeAsync(IResourceDescription resourceDescription, User user)
        {
            // Strategy: Explicitly grant, otherwise deny

            if(user.Roles.Contains(Role.Admin))
                return AuthorizationResult.Granted(user);

            switch (resourceDescription.Type)
            {
                case ResourceType.SubmitData:
                case ResourceType.GetData:
                case ResourceType.DeleteData:
                case ResourceType.Search:
                case ResourceType.CreateView:
                case ResourceType.GetView:
                case ResourceType.DeleteView:
                case ResourceType.AddValidator:
                case ResourceType.GetValidator:
                case ResourceType.SubscribeToData:
                case ResourceType.ViewCollectionInformation:
                    return await AuthorizeDataAccess((IDataResourceDescription)resourceDescription, user);
                case ResourceType.ManageValidators:
                    if (resourceDescription is ManageValidatorsResourceDescription manageValidatorsResourceDescription)
                    {
                        switch (manageValidatorsResourceDescription.Action)
                        {
                            case ValidatorManagementAction.Approve:
                            case ValidatorManagementAction.Delete:
                                return await AuthorizeDataAccess((IDataResourceDescription)resourceDescription, user);
                            case ValidatorManagementAction.ListAll:
                                // Only admins can list all validators
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                case ResourceType.ManageUser:
                    if (resourceDescription is ManageUserResourceDescription manageUserResourceDescription)
                    {
                        var userToManage = manageUserResourceDescription.UserToManage;
                        switch (manageUserResourceDescription.ActionType)
                        {
                            case UserManagementActionType.ChangePassword:
                                if(user.Roles.Contains(Role.UserManager))
                                    return AuthorizationResult.Granted(user);
                                if(user.UserName == userToManage)
                                    return AuthorizationResult.Granted(user);
                                break;
                            case UserManagementActionType.AssignRole:
                                if(user.Roles.Contains(Role.UserManager) && userToManage != user.UserName)
                                    return AuthorizationResult.Granted(user);
                                break;
                            case UserManagementActionType.Delete:
                                if(user.Roles.Contains(Role.UserManager))
                                    return AuthorizationResult.Granted(user);
                                if(user.UserName == userToManage)
                                    return AuthorizationResult.Granted(user);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                case ResourceType.GetCollectionPermissions:
                    if(user.Roles.Intersect(new []{ Role.UserManager }).Any())
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.GetGlobalRoles:
                    if(user.Roles.Intersect(new []{ Role.UserManager }).Any())
                        return AuthorizationResult.Granted(user);
                    if (resourceDescription is GetGlobalRolesResourceDescription getGlobalRolesResourceDescription)
                    {
                        if(user.UserName == getGlobalRolesResourceDescription.Username)
                            return AuthorizationResult.Granted(user);
                    }
                    break;
                case ResourceType.ViewUserProfiles:
                    if(user.Roles.Intersect(new []{Role.Viewer, Role.Analyst, Role.UserManager}).Any())
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.ProtectCollection:
                    // Only admins can protect collections
                    break;
                case ResourceType.SetDataRedirection:
                    // Only admins can set redirections
                    break;
                case ResourceType.SetCollectionOptions:
                    // Only admins can set collection options
                    break;
                case ResourceType.DeleteNotification:
                    if (resourceDescription is DeleteNotificationResourceDescription deleteNotificationResourceDescription)
                    {
                        if(user.UserName == deleteNotificationResourceDescription.NotificationUsername)
                            return AuthorizationResult.Granted(user);
                    }
                    break;
                case ResourceType.ReportData: // All users can report data
                    return AuthorizationResult.Granted(user);
                case ResourceType.ListCollections:
                    if(user.Roles.Any())
                        return AuthorizationResult.Granted(user); // Grant listing of collections to all with at least one role
                    break;
                case ResourceType.ListSubscriptions:
                    return AuthorizationResult.Granted(user); // All users can list their subscriptions
                case ResourceType.Unsubscribe:
                    if (resourceDescription is UnsubscribeResourceDescription unsubscribeResourceDescription)
                    {
                        if(user.UserName == unsubscribeResourceDescription.SubscriptionUsername)
                            return AuthorizationResult.Granted(user);
                    }
                    else if(resourceDescription is UnsubscribeAllResourceDescription)
                        return AuthorizationResult.Granted(user); // Allow all users to clear their subscriptions. See SECURITY NOTE in SubscriptionManager.UnsubscribeAll
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return AuthorizationResult.Denied();
        }

        private async Task<AuthorizationResult> AuthorizeDataAccess(IDataResourceDescription resourceDescription, User user)
        {
            var collectionName = resourceDescription.DataType;
            var roles = await GetCollectionRolesForUserAsync(collectionName, user);

            if(roles.Contains(Role.Admin))
                return AuthorizationResult.Granted(user);

            switch (resourceDescription.Type)
            {
                case ResourceType.SubmitData:
                    if(roles.Contains(Role.DataProducer) || roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.GetData:
                    if(roles.Contains(Role.Viewer) || roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.DeleteData:
                    if (resourceDescription is DeleteDataResourceDescription deleteDataResourceDescription)
                    {
                        if(deleteDataResourceDescription.OverwritingAllowed)
                            return AuthorizationResult.Granted(user);
                        if(deleteDataResourceDescription.SubmitterOfObjectToBeDeleted == user.UserName)
                            return AuthorizationResult.Granted(user);
                    }
                    break;
                case ResourceType.Search:
                    if(roles.Contains(Role.Viewer) || roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.CreateView:
                    if(roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.GetView:
                    if(roles.Contains(Role.Viewer) || roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.DeleteView:
                    if (resourceDescription is DeleteViewResourceDescription deleteViewResourceDescription)
                    {
                        if(deleteViewResourceDescription.Submitter == user.UserName)
                            return AuthorizationResult.Granted(user);
                    }
                    break;
                case ResourceType.AddValidator:
                    if(roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.GetValidator:
                    var validatorDefinition = (resourceDescription as GetValidatorResourceDescription)?.ValidatorDefinition;
                    if (validatorDefinition != null)
                    {
                        if(validatorDefinition.Submitter == user.UserName)
                            return AuthorizationResult.Granted(user);
                    }
                    break;
                case ResourceType.ManageValidators:
                    if (resourceDescription is ManageValidatorsResourceDescription manageValidatorsResourceDescription)
                    {
                        var validatorSubmitter = manageValidatorsResourceDescription.ValidatorSubmitter;
                        switch (manageValidatorsResourceDescription.Action)
                        {
                            case ValidatorManagementAction.Approve:
                                // Only admins can approve
                                break;
                            case ValidatorManagementAction.Delete:
                                // Users can delete their own validators
                                if(validatorSubmitter == user.UserName)
                                    return AuthorizationResult.Granted(user);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                case ResourceType.SubscribeToData:
                    if(roles.Contains(Role.Viewer) || roles.Contains(Role.Analyst))
                        return AuthorizationResult.Granted(user);
                    break;
                case ResourceType.ViewCollectionInformation:
                    if(roles.Intersect(new []{ Role.Viewer, Role.Analyst, Role.UserManager}).Any())
                        return AuthorizationResult.Granted(user);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return AuthorizationResult.Denied();
        }

        public async Task<List<Role>> GetCollectionRolesForUserAsync(string collectionName, User user)
        {
            if (collectionName == null)
                return user.Roles;
            var collectionMetadata = await GetCollectionMetadata(collectionName);
            var isCollectionProtected = collectionMetadata?.IsProtected ?? false;
            if (!isCollectionProtected)
                return user.Roles;
            var collectionPermissionId = CollectionPermissions.GetId(collectionName, user.UserName);
            var matchingCollectionPermissions = await collectionPermissionsCollection
                .Find(x => x.Id == collectionPermissionId)
                .FirstOrDefaultAsync();
            var collectionRoles = matchingCollectionPermissions != null ? matchingCollectionPermissions.Roles : new List<Role>();

            // Add admin and user manager roles, which always will be inherited
            if(user.Roles.Contains(Role.Admin) && !collectionRoles.Contains(Role.Admin))
                collectionRoles.Add(Role.Admin);
            if(user.Roles.Contains(Role.UserManager) && !collectionRoles.Contains(Role.UserManager)) // TODO: Reconsider this rule. UserManager inheritance is not necessary
                collectionRoles.Add(Role.UserManager);
            return collectionRoles;
        }

        public async Task AddOrUpdateCollectionMetadata(CollectionOptions collectionOptions)
        {
            var collectionName = collectionOptions.CollectionName;
            var matchingCollectionMetadata = await GetCollectionMetadata(collectionName);
            var collectionMetadata = matchingCollectionMetadata ?? new CollectionMetadata(collectionName);

            if (collectionOptions.Description != null)
                collectionMetadata.Description = collectionOptions.Description;
            if (collectionOptions.DisplayName != null)
                collectionMetadata.DisplayName = collectionOptions.DisplayName;
            if (collectionOptions.IsProtected.HasValue)
                collectionMetadata.IsProtected = collectionOptions.IsProtected.Value;
            if (collectionOptions.NonAdminUsersCanOverwriteData.HasValue)
                collectionMetadata.NonAdminUsersCanOverwriteData = collectionOptions.NonAdminUsersCanOverwriteData.Value;
            if (collectionOptions.IsHidden.HasValue)
                collectionMetadata.IsHidden = collectionOptions.IsHidden.Value;
            if (collectionOptions.IdGeneratorType.HasValue)
                collectionMetadata.IdGeneratorType = collectionOptions.IdGeneratorType.Value;

            await collectionMetadataCollection.ReplaceOneAsync(
                x => x.CollectionName == collectionName, 
                collectionMetadata, 
                new ReplaceOptions { IsUpsert = true});
        }

        public async Task<bool> IsOverwritingAllowedForCollectionAsync(string collectionName)
        {
            var matchingCollectionMetadata = await GetCollectionMetadata(collectionName);
            if (matchingCollectionMetadata == null)
                return false;
            return matchingCollectionMetadata.NonAdminUsersCanOverwriteData;
        }

        /// <summary>
        /// Gets user permissions for the specified collection.
        /// If collection is unprotected an empty list is returned.
        /// </summary>
        public async Task<List<CollectionUserPermissions>> GetAllCollectionPermissionsAsync(string collectionName)
        {
            var collectionMetadata = await GetCollectionMetadata(collectionName);
            if(collectionMetadata == null || !collectionMetadata.IsProtected)
                return new List<CollectionUserPermissions>();
            var collectionPermissions = await collectionPermissionsCollection
                .Find(x => x.CollectionName == collectionName)
                .ToListAsync();
            var collectionUserPermissions = collectionPermissions
                .Select(x => new CollectionUserPermissions(x.CollectionName, x.Username, x.Roles))
                .ToList();
            return collectionUserPermissions;
        }

        private async Task<CollectionMetadata> GetCollectionMetadata(string collectionName)
        {
            return await collectionMetadataCollection
                .Find(x => x.CollectionName == collectionName)
                .FirstOrDefaultAsync();
        }
    }
}
