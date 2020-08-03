using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using DataExplorerWpf.Views;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataProjectUploadInfoViewModel
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IClipboard clipboard;

        public DataProjectUploadInfoViewModel(
            DataProjectUploadInfo uploadInfo, 
            IDataApiClient dataApiClient,
            IClipboard clipboard)
        {
            this.dataApiClient = dataApiClient;
            this.clipboard = clipboard;

            Model = uploadInfo;
            Description = Model.Filename ?? Model.Id;

            DownloadRawDataCommand = new AsyncRelayCommand(DownloadRawData, () => Model.RawData != null);
            ShowDerivedDataCommand = new RelayCommand(ShowDerivedData, () => Model.DerivedData.Any());
        }

        public DataProjectUploadInfo Model { get; }
        public DateTime UploadTimestamp => Model.UploadTimestamp;
        public string Description { get; }

        public IAsyncCommand DownloadRawDataCommand { get; }
        public ICommand ShowDerivedDataCommand { get; }
        public ApiConfiguration DataApiConfiguration => dataApiClient.ApiConfiguration;

        private async Task DownloadRawData()
        {
            await DataDownloadHelpers.Download(Model.RawData, dataApiClient);
        }

        private async void ShowDerivedData()
        {
            try
            {
                var dataObjectLoader = new DataObjectLoader(dataApiClient, clipboard, new DummyCollectionSwitcher());
                var dataObjects = await dataObjectLoader.Load(Model.DerivedData, dataObject => { });
                var windowTitle = $"Derived data for upload '{Model.Filename ?? Model.Id}'";
                var dataWindowViewModel = new DataWindowViewModel(windowTitle, dataObjects);
                var dataWindow = new DataWindow
                {
                    ViewModel = dataWindowViewModel
                };
                dataWindow.ShowDialog();
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Could not show derived data: {e.InnermostException().Message}");
            }
        }
    }
}
