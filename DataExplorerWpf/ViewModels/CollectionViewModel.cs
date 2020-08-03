using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;
using DataExplorerWpf.Views;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CollectionViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;

        public CollectionViewModel(
            CollectionInformation collectionInformation,
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            CollectionInformation = collectionInformation;
            this.dataApiClient = dataApiClient;
            
            this.userDatabase = userDatabase;
            Permission = TranslatePermission(collectionInformation.UserRoles);

            CanEditCollectionSettings = collectionInformation.UserRoles.Intersect(new[] {Role.UserManager, Role.Admin}).Any();
            OpenCollectionSettingsWindowCommand = new AsyncRelayCommand(OpenCollectionSettingsWindow, () => CanEditCollectionSettings);
        }


        private CollectionInformation collectionInformation;
        public CollectionInformation CollectionInformation
        {
            get => collectionInformation;
            private set
            {
                collectionInformation = value;
                OnPropertyChanged();
            }
        }
        public string DisplayName => CollectionInformation.DisplayName ?? CollectionName;
        public string CollectionName => CollectionInformation.CollectionName;
        public bool IsProtected => CollectionInformation.IsProtected;
        public bool IsHidden => CollectionInformation.IsHidden;
        public bool CanEditCollectionSettings { get; }
        public PermissionType Permission { get; }
        public IAsyncCommand OpenCollectionSettingsWindowCommand { get; }

        private async Task OpenCollectionSettingsWindow()
        {
            List<CollectionUserPermissions> collectionPermissions;
            try
            {
                collectionPermissions = await dataApiClient.GetCollectionPermissions(CollectionName);
            }
            catch (ApiException apiException)
            {
                if(apiException.StatusCode == HttpStatusCode.Unauthorized)
                    collectionPermissions = new List<CollectionUserPermissions>();
                else
                    throw;
            }
            var collectionSettingsViewModel = new CollectionSettingsViewModel(
                CollectionInformation, 
                collectionPermissions,
                dataApiClient,
                userDatabase);
            var collectionSettingsWindow = new CollectionSettingsWindow
            {
                ViewModel = collectionSettingsViewModel
            };
            if(collectionSettingsWindow.ShowDialog() != true)
                return;
            CollectionInformation = await dataApiClient.GetCollectionInformationAsync(CollectionName);
        }

        private static PermissionType TranslatePermission(ICollection<Role> roles)
        {
            if (roles.Contains(Role.Admin))
                return PermissionType.ReadWrite;
            if (roles.Contains(Role.Analyst))
                return PermissionType.ReadWrite;
            if (roles.Contains(Role.Viewer) && roles.Contains(Role.DataProducer))
                return PermissionType.ReadWrite;
            if (roles.Contains(Role.Viewer))
                return PermissionType.Read;
            if (roles.Contains(Role.DataProducer))
                return PermissionType.Write;
            return PermissionType.None;
        }
    }
}