using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.DataStructures
{
    public class DataPermissionCheckerResult
    {
        private DataPermissionCheckerResult(
            bool hasSufficientPermissions, 
            string firstFailingCollection = null, 
            IList<Role> firstFailingCollectionRoles = null,
            IList<Role> requiredRoles = null)
        {
            HasSufficientPermissions = hasSufficientPermissions;
            if(!HasSufficientPermissions)
            {
                FirstFailingCollection = firstFailingCollection;
                FirstFailingCollectionRoles = firstFailingCollectionRoles;
                RequiredRoles = requiredRoles;
            }
        }

        public static DataPermissionCheckerResult Pass()
        {
            return new DataPermissionCheckerResult(true);
        }

        public static DataPermissionCheckerResult Fail(string collectionName, IList<Role> collectionRoles, IList<Role> requiredRoles)
        {
            return new DataPermissionCheckerResult(false, collectionName, collectionRoles, requiredRoles);
        }

        public bool HasSufficientPermissions { get; }
        public IList<Role> RequiredRoles { get; }
        public string FirstFailingCollection { get; }
        public IList<Role> FirstFailingCollectionRoles { get; }
    }
}
