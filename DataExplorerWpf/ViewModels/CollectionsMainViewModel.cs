using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CollectionsMainViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly DataVisualizer dataVisualizer;
        private readonly IClipboard clipboard;
        private readonly ICollectionSwitcher collectionSwitcher;
        private readonly Dictionary<string, CollectionExplorationViewModel> collectionExplorationViewModels = new Dictionary<string, CollectionExplorationViewModel>();

        public CollectionsMainViewModel(
            IDataApiClient dataApiClient,
            DataVisualizer dataVisualizer,
            IReadonlyObjectDatabase<UserProfile> userDatabase,
            IClipboard clipboard)
        {
            this.dataApiClient = dataApiClient;
            this.dataVisualizer = dataVisualizer;
            this.clipboard = clipboard;
            collectionSwitcher = new CollectionSwitcher(SwitchToCollection, Constants.DefaultQuery);
            CollectionListViewModel = new CollectionListViewModel(dataApiClient, userDatabase);
            CollectionListViewModel.PropertyChanged += CollectionListViewModel_PropertyChanged;
        }

        public async Task Reload()
        {
            await CollectionListViewModel.Reload();
        }

        public CollectionListViewModel CollectionListViewModel { get; }

        private CollectionExplorationViewModel selectedCollectionExplorationViewModel;
        public CollectionExplorationViewModel SelectedCollectionExplorationViewModel
        {
            get => selectedCollectionExplorationViewModel;
            private set
            {
                selectedCollectionExplorationViewModel = value;
                OnPropertyChanged();
            }
        }

        private Visibility collectionExplorerVisibility = Visibility.Hidden;
        public Visibility CollectionExplorerVisibility
        {
            get => collectionExplorerVisibility;
            private set
            {
                collectionExplorerVisibility = value;
                OnPropertyChanged();
            }
        }

        private void SwitchToCollection(string collectionName, string query)
        {
            var matchingCollection = CollectionListViewModel.Collections.FirstOrDefault(x => x.CollectionName == collectionName);
            if(matchingCollection == null)
            {
                StaticMessageBoxSpawner.Show($"Collection '{collectionName}' doesn't exist");
                return;
            }
            if (!collectionExplorationViewModels.ContainsKey(collectionName))
                CreateCollectionExplorationViewModel(matchingCollection);
            var collectionExplorationViewModel = collectionExplorationViewModels[collectionName];
            collectionExplorationViewModel.QueryEditorViewModel.Query = query;
            collectionExplorationViewModel.QueryEditorViewModel.RunQueryCommand.ExecuteAsync(null).ConfigureAwait(false);
            CollectionListViewModel.SelectedCollection = matchingCollection;
        }

        private void CollectionListViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.CollectionListViewModel.SelectedCollection))
            {
                if(CollectionListViewModel.SelectedCollection != null)
                {
                    var collectionName = CollectionListViewModel.SelectedCollection.CollectionName;
                    if (collectionExplorationViewModels.ContainsKey(collectionName))
                    {
                        SelectedCollectionExplorationViewModel = collectionExplorationViewModels[collectionName];
                    }
                    else
                    {
                        var collectionExplorationViewModel = CreateCollectionExplorationViewModel(CollectionListViewModel.SelectedCollection);
                        SelectedCollectionExplorationViewModel = collectionExplorationViewModel;
                        SelectedCollectionExplorationViewModel.QueryEditorViewModel.RunQueryCommand.Execute(null);
                    }
                    CollectionExplorerVisibility = Visibility.Visible;
                }
                else
                {
                    SelectedCollectionExplorationViewModel = null;
                    CollectionExplorerVisibility = Visibility.Hidden;
                }
            }
        }

        private CollectionExplorationViewModel CreateCollectionExplorationViewModel(CollectionViewModel collection)
        {
            var collectionExplorationViewModel = new CollectionExplorationViewModel(
                dataApiClient,
                collection.CollectionInformation,
                dataVisualizer,
                clipboard,
                collectionSwitcher);
            collectionExplorationViewModels.Add(collection.CollectionName, collectionExplorationViewModel);
            return collectionExplorationViewModel;
        }
    }
}