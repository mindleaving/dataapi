using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;
using DataExplorerWpf.Views;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CollectionSettingsViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;

        public CollectionSettingsViewModel(
            CollectionInformation collectionInformation,
            List<CollectionUserPermissions> collectionPermissions,
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            this.dataApiClient = dataApiClient;
            this.userDatabase = userDatabase;

            CollectionName = collectionInformation.CollectionName;
            IsAdmin = collectionInformation.UserRoles.Contains(Role.Admin);
            IsUserManager = collectionInformation.UserRoles.Intersect(new[] {Role.Admin, Role.UserManager}).Any();
            DisplayName = collectionInformation.DisplayName ?? collectionInformation.CollectionName;
            Description = collectionInformation.Description;
            SelectedIdGeneratorType = collectionInformation.IdGeneratorType;
            NonAdminUsersCanOverwriteData = collectionInformation.NonAdminUsersCanOverwriteData;
            IsProtected = collectionInformation.IsProtected;
            IsHidden = collectionInformation.IsHidden;
            var currentUser = dataApiClient.LoggedInUsername;
            var userPermissionViewModels = BuildUserPermissionsViewModels(collectionInformation, collectionPermissions, currentUser);
            UserPermissions = new ObservableCollection<UserPermissionsViewModel>(userPermissionViewModels);
            Validators = collectionInformation.ValidatorDefinitions.Select(x => new ValidatorDefinitionViewModel(x)).ToList();

            AddUserCommand = new RelayCommand(AddUser, () => CanEditUserPermissions);
            DeleteUserCommand = new RelayCommand(DeleteUser, () => CanEditUserPermissions);
            SaveCommand = new AsyncRelayCommand<IClosable>(Save);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
        }

        public string CollectionName { get; }

        public bool IsAdmin { get; }
        public bool IsUserManager { get; }
        private bool canEditUserPermissions;
        public bool CanEditUserPermissions
        {
            get => canEditUserPermissions;
            private set
            {
                canEditUserPermissions = value;
                OnPropertyChanged();
            }
        }

        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                displayName = value;
                OnPropertyChanged();
            }
        }

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        public IList<IdGeneratorType> IdGeneratorTypes { get; } = EnumExtensions.GetValues<IdGeneratorType>().ToList();

        private IdGeneratorType selectedIdGeneratorType;
        public IdGeneratorType SelectedIdGeneratorType
        {
            get => selectedIdGeneratorType;
            set
            {
                selectedIdGeneratorType = value;
                OnPropertyChanged();
            }
        }

        private bool nonAdminUsersCanOverwriteData;
        public bool NonAdminUsersCanOverwriteData
        {
            get => nonAdminUsersCanOverwriteData;
            set
            {
                nonAdminUsersCanOverwriteData = value;
                OnPropertyChanged();
            }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                isHidden = value;
                OnPropertyChanged();
            }
        }

        private bool isProtected;
        public bool IsProtected
        {
            get => isProtected;
            set
            {
                isProtected = value;
                CanEditUserPermissions = IsUserManager && IsProtected;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserPermissionsViewModel> UserPermissions { get; }
        private UserPermissionsViewModel selectedUser;
        public UserPermissionsViewModel SelectedUser
        {
            get => selectedUser;
            set
            {
                selectedUser = value;
                OnPropertyChanged();
            }
        }

        public List<ValidatorDefinitionViewModel> Validators { get; }

        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public IAsyncCommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void AddUser()
        {
            var userPermissionEditViewModel = new UserSelectionViewModel(userDatabase);
            var userPermissionEditWindow = new UserSelectionWindow
            {
                ViewModel = userPermissionEditViewModel
            };
            if(userPermissionEditWindow.ShowDialog() != true)
                return;
            var username = userPermissionEditViewModel.SelectedUser.Username;
            var isCurrentUser = username == dataApiClient.LoggedInUsername;
            var newUser = new UserPermissionsViewModel(username, isCurrentUser, new List<Role>());
            UserPermissions.Add(newUser);
        }

        private void DeleteUser()
        {
            if(SelectedUser == null)
                return;
            var confirmationResult = StaticMessageBoxSpawner.Show(
                $"Delete user '{SelectedUser.Username}' from collection?",
                "Delete user?",
                MessageBoxButtons.YesNo);
            if (confirmationResult != MessageBoxResult.Yes)
                return;
            UserPermissions.Remove(SelectedUser);
        }

        private async Task Save(IClosable closable)
        {
            try
            {
                if (IsAdmin)
                {
                    var collectionOptions = new CollectionOptions(CollectionName)
                    {
                        DisplayName = DisplayName,
                        Description = Description,
                        NonAdminUsersCanOverwriteData = NonAdminUsersCanOverwriteData,
                        IsProtected = IsProtected,
                        IsHidden = IsHidden,
                        IdGeneratorType = SelectedIdGeneratorType
                    };
                    dataApiClient.SetCollectionOptions(collectionOptions);

                    foreach (var validator in Validators)
                    {
                        var hasBeenApproved = !validator.Model.IsApproved && validator.IsApproved;
                        if (hasBeenApproved)
                        {
                            await dataApiClient.ApproveValidatorAsync(validator.Model.Id);
                            continue;
                        }
                        var hasBeenUnapproved = validator.Model.IsApproved && !validator.IsApproved;
                        if (hasBeenUnapproved)
                        {
                            await dataApiClient.UnapproveValidatorAsync(validator.Model.Id);
                        }
                    }
                }
                if (CanEditUserPermissions)
                {
                    foreach (var userPermission in UserPermissions.Where(x => x.CanSetAnyPermission))
                    {
                        dataApiClient.SetCollectionRoleForUser(
                            userPermission.Username,
                            userPermission.Roles,
                            CollectionName);
                    }
                }
                closable.Close(true);
            }
            catch(Exception e)
            {
                if (e is ApiException apiException)
                {
                    if (apiException.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        StaticMessageBoxSpawner.Show("You are not authorized to change collection settings");
                        return;
                    }
                }
                StaticMessageBoxSpawner.Show($"Cannot change collection settings: {e.InnermostException().Message}");
            }
        }

        private static List<UserPermissionsViewModel> BuildUserPermissionsViewModels(
            CollectionInformation collectionInformation,
            List<CollectionUserPermissions> collectionPermissions,
            string currentUser)
        {
            var userPermissionViewModels = collectionPermissions
                .Select(
                    userPermission =>
                    {
                        var isCurrentUser = userPermission.Username == currentUser;
                        return new UserPermissionsViewModel(userPermission.Username, isCurrentUser, userPermission.Roles);
                    })
                .ToList();
            if (userPermissionViewModels.All(vm => vm.Username != currentUser))
            {
                userPermissionViewModels.Add(new UserPermissionsViewModel(currentUser, true, collectionInformation.UserRoles));
            }

            return userPermissionViewModels;
        }
    }
}
