using System;
using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.Service.AccessManagement
{
    public static class RolesParser
    {
        public static bool TryParse(string rolesString, out List<Role> roles)
        {
            var splittedRoles = rolesString.Split('|');
            roles = new List<Role>();
            foreach (var roleString in splittedRoles)
            {
                if(string.IsNullOrEmpty(roleString))
                    continue;
                if (!Enum.TryParse<Role>(roleString, out var role))
                {
                    roles = null;
                    return false;
                }
                roles.Add(role);
            }
            return true;
        }
    }
}
