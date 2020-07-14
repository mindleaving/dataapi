using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CollectionListViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<UserProfile> userDatabase;

        public CollectionListViewModel(
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<UserProfile> userDatabase)
        {
            this.dataApiClient = dataApiClient;
            this.userDatabase = userDatabase;

            RefreshCollectionListCommand = new AsyncRelayCommand(Reload);
        }

        private string collectionSearchPattern;
        public string CollectionSearchPattern
        {
            get => collectionSearchPattern;
            set
            {
                collectionSearchPattern = value;
                FilterCollections();
                OnPropertyChanged();
            }
        }

        private bool includeHidden;
        public bool IncludeHidden
        {
            get => includeHidden;
            set
            {
                includeHidden = value;
                FilterCollections();
                OnPropertyChanged();
            }
        }

        private List<CollectionViewModel> loadedCollections;
        private List<CollectionViewModel> collections;
        public List<CollectionViewModel> Collections
        {
            get => collections;
            private set
            {
                collections = value;
                OnPropertyChanged();
            }
        }

        private CollectionViewModel selectedCollection;
        public CollectionViewModel SelectedCollection
        {
            get => selectedCollection;
            set
            {
                selectedCollection = value;
                OnPropertyChanged();
            }
        }
        public IAsyncCommand RefreshCollectionListCommand { get; }

        public async Task Reload()
        {
            try
            {
                var collectionInformations = await dataApiClient.ListCollectionsAsync(includeHidden: true);
                loadedCollections = collectionInformations
                    .Select(collectionInformation => new CollectionViewModel(collectionInformation, dataApiClient, userDatabase))
                    .OrderBy(collectionInfo => collectionInfo.CollectionName)
                    .ToList();
            }
            catch (ApiException apiException)
            {
                StaticMessageBoxSpawner.Show($"Could not load list of collections: {apiException.Message}");
                loadedCollections = new List<CollectionViewModel>();
            }
            CollectionSearchPattern = string.Empty;  // Implicitly updates Collections
        }

        private void FilterCollections()
        {
            var shownCollections = loadedCollections.AsEnumerable();
            shownCollections = shownCollections.Where(collection => IncludeHidden || !collection.IsHidden);
            if (!string.IsNullOrEmpty(collectionSearchPattern))
            {
                shownCollections = shownCollections.Where(MatchesSearchPattern);
            }
            Collections = shownCollections.ToList();
        }

        private bool MatchesSearchPattern(CollectionViewModel collection)
        {
            var lowerCollectionName = collection.CollectionName.ToLowerInvariant();
            return lowerCollectionName.Contains(collectionSearchPattern.ToLowerInvariant());
        }
    }
}