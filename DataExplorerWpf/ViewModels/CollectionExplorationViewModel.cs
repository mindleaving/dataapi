using System.Collections.Generic;
using DataAPI.Client;
using DataAPI.DataStructures;
using Newtonsoft.Json.Linq;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class CollectionExplorationViewModel : NotifyPropertyChangedBase
    {
        public CollectionExplorationViewModel(
            IDataApiClient dataApiClient,
            CollectionInformation collectionInformation,
            DataVisualizer dataVisualizer,
            IClipboard clipboard,
            ICollectionSwitcher collectionSwitcher)
        {
            CollectionInformation = collectionInformation;
            DataObjectListViewModel = new DataObjectListViewModel(collectionInformation, dataApiClient, dataVisualizer, clipboard, collectionSwitcher);
            QueryEditorViewModel = new QueryEditorViewModel(dataApiClient, collectionInformation.CollectionName);
            QueryEditorViewModel.JsonSearchFinished += QueryEditorViewModel_JsonSearchFinished;
        }

        public CollectionInformation CollectionInformation { get; }
        public DataObjectListViewModel DataObjectListViewModel { get; }
        public QueryEditorViewModel QueryEditorViewModel { get; }
        
        private void QueryEditorViewModel_JsonSearchFinished(object sender, List<JObject> searchResults)
        {
            DataObjectListViewModel.SetSearchResult(searchResults, DataObjectListViewModel.IncludeMetadata);
        }
    }
}