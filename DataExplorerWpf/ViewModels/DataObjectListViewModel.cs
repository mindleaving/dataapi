using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataObjectListViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly DataVisualizer dataVisualizer;
        private readonly IClipboard clipboard;
        private readonly ICollectionSwitcher collectionSwitcher;
        private List<JObject> searchResults;

        public DataObjectListViewModel(
            CollectionInformation collectionInformation,
            IDataApiClient dataApiClient, 
            DataVisualizer dataVisualizer,
            IClipboard clipboard,
            ICollectionSwitcher collectionSwitcher)
        {
            this.dataApiClient = dataApiClient;
            
            this.dataVisualizer = dataVisualizer;
            this.clipboard = clipboard;
            this.collectionSwitcher = collectionSwitcher;
            CollectionInformation = collectionInformation;
            CanVisualize = dataVisualizer.CanVisualize(CollectionInformation.CollectionName);
            ExportCommand = new RelayCommand(Export, CanExport);
            VisualizeCommand = new RelayCommand(Visualize, () => CanVisualize);
        }

        public CollectionInformation CollectionInformation { get; }

        private bool includeMetadata;
        public bool IncludeMetadata
        {
            get => includeMetadata;
            set
            {
                if(value == includeMetadata)
                    return;
                includeMetadata = value;
                Items = new ObservableCollection<DataObjectViewModel>(
                    DataObjectBuilder.BuildItems(
                        searchResults,
                        includeMetadata,
                        CollectionInformation.CollectionName,
                        dataApiClient,
                        clipboard,
                        collectionSwitcher,
                        DeleteDataObject));
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataObjectViewModel> items;
        public ObservableCollection<DataObjectViewModel> Items
        {
            get => items;
            private set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExportCommand { get; }
        public ICommand VisualizeCommand { get; }

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void SetSearchResult(
            List<JObject> searchResults,
            bool includeMetadata)
        {
            this.searchResults = searchResults;
            this.includeMetadata = includeMetadata;
            Items = new ObservableCollection<DataObjectViewModel>(
                DataObjectBuilder.BuildItems(
                    searchResults,
                    includeMetadata, 
                    CollectionInformation.CollectionName,
                    dataApiClient,
                    clipboard,
                    collectionSwitcher,
                    DeleteDataObject));
            OnPropertyChanged(nameof(IncludeMetadata));
        }

        private void DeleteDataObject(DataObjectViewModel obj)
        {
            Items.Remove(obj);
        }

        private bool CanExport()
        {
            return searchResults != null 
                   && searchResults.Count > 0;
        }

        private void Export()
        {
            var fileDialog = new SaveFileDialog
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "JSON|*.json|All files|*.*",
                FileName = $"{CollectionInformation.CollectionName}_{DateTime.Now:yyyy-MM-dd_HHmm}.json"
            };
            if(fileDialog.ShowDialog() != true)
                return;
            try
            {
                var jTokens = GetSearchResult(searchResults, IncludeMetadata);
                var exportJson = jTokens.Select(jToken => jToken.ToString());
                File.WriteAllLines(fileDialog.FileName, exportJson);
                StaticMessageBoxSpawner.Show("Items successfully exported");
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Error: {e.Message}");
            }
        }

        public bool CanVisualize { get; }

        private void Visualize()
        {
            try
            {
                var jTokens = GetSearchResult(searchResults, false);
                var json = jTokens.Select(jToken => jToken.ToString());
                dataVisualizer.Visualize(CollectionInformation.CollectionName, json);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Cannot visualize: {e.InnermostException().Message}");
            }
        }

        private static IEnumerable<JToken> GetSearchResult(IEnumerable<JObject> searchResults, bool includeMetadata)
        {
            if (includeMetadata)
            {
                foreach (var jObject in searchResults)
                    yield return jObject;
            }
            else
            {
                foreach (var jObject in searchResults)
                {
                    if (jObject.ContainsKey("Data"))
                        yield return jObject["Data"];
                    else
                        yield return jObject;
                }
            }
        }
    }
}
