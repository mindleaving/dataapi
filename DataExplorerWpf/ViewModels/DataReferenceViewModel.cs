using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.ViewModels;
using SharedWpfControls.Properties;

namespace DataExplorerWpf.ViewModels
{
    public class DataReferenceViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;

        public DataReferenceViewModel(
            [NotNull] DataReference dataReference,
            [NotNull] IDataApiClient dataApiClient)
        {
            Model = dataReference ?? throw new ArgumentNullException(nameof(dataReference));
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));

            DownloadCommand = new AsyncRelayCommand(Download);
        }

        public DataReference Model { get; }

        private List<ShortId> shortIds;
        public List<ShortId> ShortIds => shortIds ?? (shortIds = LoadShortIds());

        public IAsyncCommand DownloadCommand { get; }
        public ApiConfiguration DataApiConfiguration => dataApiClient.ApiConfiguration;

        private async Task Download()
        {
            await DataDownloadHelpers.Download(Model, dataApiClient);
        }

        private List<ShortId> LoadShortIds()
        {
            return Task.Run(
                () =>
                    dataApiClient.GetManyAsync<ShortId>(
                            $"Data.{nameof(ShortId.CollectionName)} = '{Model.DataType}' AND Data.{nameof(ShortId.OriginalId)} = '{Model.Id}'")
                        .Result).Result;
        }
    }
}
