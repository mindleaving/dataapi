using System.Collections.Generic;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class UserPermissionsViewModel : NotifyPropertyChangedBase
    {
        public UserPermissionsViewModel(string username, bool isCurrentUser, List<Role> roles)
        {
            Username = username;
            if (roles.Contains(Role.Admin))
                IsAdmin = true;
            else
            {
                if (roles.Contains(Role.Viewer))
                    CanView = true;
                if (roles.Contains(Role.DataProducer))
                    CanWrite = true;
                if (roles.Contains(Role.Analyst))
                {
                    CanView = true;
                    CanWrite = true;
                }
                if (roles.Contains(Role.UserManager))
                    IsUserManager = true;
            }
            CanSetAnyPermission = !isCurrentUser;
        }

        public string Username { get; }
        public List<Role> Roles
        {
            get
            {
                if (IsAdmin)
                    return new List<Role> {Role.Admin};
                var roles = new List<Role>();
                if(CanView)
                    roles.Add(Role.Viewer);
                if(CanWrite)
                    roles.Add(Role.DataProducer);
                if(IsUserManager)
                    roles.Add(Role.UserManager);
                return roles;
            }
        }

        private bool canView;
        public bool CanView
        {
            get => canView;
            set
            {
                canView = value;
                OnPropertyChanged();
            }
        }
        private bool canWrite;
        public bool CanWrite
        {
            get => canWrite;
            set
            {
                canWrite = value;
                OnPropertyChanged();
            }
        }
        private bool isUserManager;
        public bool IsUserManager
        {
            get => isUserManager;
            set
            {
                isUserManager = value;
                OnPropertyChanged();
            }
        }
        private bool isAdmin;
        public bool IsAdmin
        {
            get => isAdmin;
            set
            {
                isAdmin = value;
                if (isAdmin)
                {
                    CanView = true;
                    CanWrite = true;
                    IsUserManager = true;
                }
                CanSetNonAdminPermissions = !isAdmin;
                OnPropertyChanged();
            }
        }

        private bool canSetNonAdminPermissions = true;
        public bool CanSetNonAdminPermissions
        {
            get => CanSetAnyPermission && canSetNonAdminPermissions;
            private set
            {
                canSetNonAdminPermissions = value;
                OnPropertyChanged();
            }
        }

        public bool CanSetAnyPermission { get; }
    }
}