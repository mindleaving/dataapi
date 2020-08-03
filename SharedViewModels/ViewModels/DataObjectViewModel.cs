using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.DataIo;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class DataObjectViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly Action<DataObjectViewModel> deletionCallback;

        public DataObjectViewModel(
            JObjectViewModel data, 
            string dataType, 
            string id,
            IDataApiClient dataApiClient,
            Action<DataObjectViewModel> deletionCallback)
        {
            this.dataApiClient = dataApiClient;
            
            this.deletionCallback = deletionCallback;
            DataType = dataType;
            Id = id;
            DisplayName = dataType == nameof(DataBlob)
                ? data.JToken.Value<string>(nameof(DataBlob.Filename)) ?? Id
                : Id;
            Data = new[] {data};
            Header = data.Name;
            DeleteCommand = new AsyncRelayCommand(Delete, () => Id != null);
        }

        public string DataType { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public IList<JObjectViewModel> Data { get; }
        public string Header { get; }

        public IAsyncCommand DeleteCommand { get; }

        private async Task Delete()
        {
            if (StaticMessageBoxSpawner.Show(
                    $"Are you sure you want to delete '{Id}'",
                    "Delete?",
                    MessageBoxButtons.YesNo) 
                != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await dataApiClient.DeleteAsync(DataType, Id);
                deletionCallback(this);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Could not delete '{Id}': {e.InnermostException().Message}");
            }
        }
    }
}
