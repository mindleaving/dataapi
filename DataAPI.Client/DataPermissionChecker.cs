using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.Client
{
    /// <summary>
    /// Checks for sufficient permissions to data collections.
    /// Use this for your application to check that the user logging
    /// in has enough permissions to run the application.
    /// </summary>
    public static class DataPermissionChecker
    {
        /// <summary>
        /// Checks collections for sufficient permissions.
        /// </summary>
        /// <param name="dataApiClient">DataAPI client</param>
        /// <param name="requiredCollectionPermissions">Map of required permissions. Data type as key, list of permissions as value.</param>
        /// <returns>
        /// Result containing boolean indicating sufficient permissions and,
        /// if false, the first collection with insufficient permissions, the actual roles and the required roles.
        /// </returns>
        public static async Task<DataPermissionCheckerResult> Check(
            IDataApiClient dataApiClient,
            Dictionary<string, IList<Role>> requiredCollectionPermissions)
        {
            foreach (var kvp in requiredCollectionPermissions)
            {
                var collectionName = kvp.Key;
                var requiredPermissions = kvp.Value;
                try
                {
                    var collectionInfomration = await dataApiClient.GetCollectionInformationAsync(collectionName);
                    if (!HashSufficientPermissions(collectionInfomration.UserRoles, requiredPermissions))
                    {
                        return DataPermissionCheckerResult.Fail(collectionName, collectionInfomration.UserRoles, requiredPermissions);
                    }
                }
                catch (ApiException apiException)
                {
                    if(apiException.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return DataPermissionCheckerResult.Fail(collectionName, new Role[] {}, requiredPermissions);
                    }
                    throw;
                }
            }

            return DataPermissionCheckerResult.Pass();
        }

        private static bool HashSufficientPermissions(IList<Role> collectionRoles, IList<Role> requiredRoles)
        {
            foreach (var requiredRole in requiredRoles)
            {
                switch (requiredRole)
                {
                    case Role.Viewer:
                        if (!collectionRoles.Intersect(new[] {Role.Viewer, Role.Analyst, Role.Admin}).Any())
                            return false;
                        break;
                    case Role.DataProducer:
                        if (!collectionRoles.Intersect(new[] {Role.DataProducer, Role.Analyst, Role.Admin}).Any())
                            return false;
                        break;
                    case Role.Analyst:
                        if (!collectionRoles.Contains(Role.Analyst) 
                            && !collectionRoles.Contains(Role.Admin)
                            && (!collectionRoles.Contains(Role.Viewer) || !collectionRoles.Contains(Role.DataProducer)))
                            return false;
                        break;
                    case Role.UserManager:
                        if (!collectionRoles.Intersect(new[] { Role.UserManager, Role.Admin}).Any())
                            return false;
                        break;
                    case Role.Admin:
                        if (!collectionRoles.Contains(Role.Admin))
                            return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }
    }
}
