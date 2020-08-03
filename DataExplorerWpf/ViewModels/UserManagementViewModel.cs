using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;
using DataExplorerWpf.Workflow;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class UserManagementViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;

        public UserManagementViewModel(
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            this.dataApiClient = dataApiClient;
            
            try
            {
                var userProfileLoader = new UserProfileLoader(dataApiClient, userDatabase);
                var userProfiles = Task.Run(() => userProfileLoader.LoadAsync()).Result
                    .OrderBy(x => x.Username);
                UserProfiles = new ObservableCollection<UserProfileViewModel>(userProfiles);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Failed to load user profiles: {e.InnermostException().Message}");
                UserProfiles = new ObservableCollection<UserProfileViewModel>();
            }
            CreateUserViewModel = new CreateUserViewModel(userDatabase, dataApiClient);
            CreateUserViewModel.UserCreated += (sender, userProfile) => UserProfiles.Add(userProfile);

            SaveUserDetailsCommand = new RelayCommand(SaveUserDetails, () => SelectedUserProfile != null);
            AddRoleCommand = new RelayCommand(AddRole, CanEditUser);
            RemoveRoleCommand = new RelayCommand(RemoveRole, CanEditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanEditUser);
        }

        public ObservableCollection<UserProfileViewModel> UserProfiles { get; }

        private UserProfileViewModel selectedUserProfile;
        public UserProfileViewModel SelectedUserProfile
        {
            get => selectedUserProfile;
            set
            {
                selectedUserProfile = value;
                OnPropertyChanged();
            }
        }

        public CreateUserViewModel CreateUserViewModel { get; }

        public List<Role> Roles { get; } = EnumExtensions.GetValues<Role>().ToList();
        private Role selectedRole;
        public Role SelectedRole
        {
            get => selectedRole;
            set
            {
                selectedRole = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveUserDetailsCommand { get; }
        public ICommand AddRoleCommand { get; }
        public ICommand RemoveRoleCommand { get; }
        public ICommand DeleteUserCommand { get; }

        private bool CanEditUser()
        {
            return SelectedUserProfile != null 
                   && SelectedUserProfile.Username != dataApiClient.LoggedInUsername;
        }

        private void SaveUserDetails()
        {
            try
            {
                throw new NotSupportedException("Currently not implemented in DataAPI");
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Failed to store user details: {e.InnermostException().Message}");
            }
        }

        private void AddRole()
        {
            var username = SelectedUserProfile.Username;
            try
            {
                dataApiClient.AddGlobalRoleToUser(username, SelectedRole);
                StaticMessageBoxSpawner.Show($"Role '{SelectedRole}' successfully added to user '{username}'");
                SelectedUserProfile.Roles.Add(SelectedRole);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Failed to add role to user '{username}': {e.InnermostException().Message}");
            }
        }

        private void RemoveRole()
        {
            var username = SelectedUserProfile.Username;
            try
            {
                dataApiClient.RemoveGlobalRoleFromUser(username, SelectedRole);
                StaticMessageBoxSpawner.Show($"Role '{SelectedRole}' successfully removed from user '{username}'");
                SelectedUserProfile.Roles.Remove(SelectedRole);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Failed to remove role from user '{username}': {e.InnermostException().Message}");
            }
        }

        private void DeleteUser()
        {
            var username = SelectedUserProfile.Username;
            var confirmation = StaticMessageBoxSpawner.Show(
                $"Are you sure you want to delete user '{username}'?",
                "Delete user?",
                MessageBoxButtons.YesNo);
            if(confirmation != MessageBoxResult.Yes)
                return;
            try
            {
                dataApiClient.DeleteUser(username);
                StaticMessageBoxSpawner.Show($"User '{username}' successfully deleted");
                UserProfiles.Remove(SelectedUserProfile);
                SelectedUserProfile = null;
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Failed to delete user '{username}': {e.InnermostException().Message}");
            }
        }
    }
}
