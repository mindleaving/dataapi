using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class ShortIdEditViewModel : NotifyPropertyChangedBase
    {
        private readonly IObjectDatabase<ShortId> shortIdDatabase;
        private readonly IDataApiClient dataApiClient;

        public ShortIdEditViewModel(
            DataReference dataReference,
            IObjectDatabase<ShortId> shortIdDatabase,
            IDataApiClient dataApiClient)
        {
            OriginalId = dataReference.Id;
            DataType = dataReference.DataType;
            
            this.shortIdDatabase = shortIdDatabase;
            this.dataApiClient = dataApiClient;
            ExistingShortIds = new ObservableCollection<ShortId>();
            Task.Run(() => LoadExistingShortIds(dataReference));
            StoreShortIdCommand = new AsyncRelayCommand(StoreShortId, () => !string.IsNullOrEmpty(UserSpecificId));
        }

        private async Task LoadExistingShortIds(DataReference dataReference)
        {
            var query = $"Data.{nameof(DataAPI.DataStructures.DataManagement.ShortId.OriginalId)} = '{dataReference.Id}' " +
                        $"AND Data.{nameof(DataAPI.DataStructures.DataManagement.ShortId.CollectionName)} = '{dataReference.DataType}'";
            var shortIds = await shortIdDatabase.GetManyAsync(query);
            shortIds.ForEach(x => ExistingShortIds.Add(x));
        }

        public string OriginalId { get; }
        public string DataType { get; }
        public string Username => dataApiClient.LoggedInUsername;
        private string userSpecificId;
        public string UserSpecificId
        {
            get => userSpecificId;
            set
            {
                if (!Regex.IsMatch(value, "^[a-zA-Z0-9_-]+$"))
                    throw new FormatException();
                userSpecificId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShortId));
            }
        }
        public string ShortId => $"{Username}_{UserSpecificId}";
        public ObservableCollection<ShortId> ExistingShortIds { get; }
        public ApiConfiguration DataApiConfiguration => dataApiClient.ApiConfiguration;

        public IAsyncCommand StoreShortIdCommand { get; }

        private async Task StoreShortId()
        {
            if (await shortIdDatabase.ExistsAsync(ShortId))
            {
                var existingShortId = await shortIdDatabase.GetFromIdAsync(ShortId);
                StaticMessageBoxSpawner.Show($"Short ID '{ShortId}' already exists and points to data of type '{existingShortId.CollectionName}'");
                return;
            }
            var shortIdInstance = new ShortId(ShortId, DataType, OriginalId);
            await shortIdDatabase.StoreAsync(shortIdInstance);
            ExistingShortIds.Add(shortIdInstance);
            StaticMessageBoxSpawner.Show($"Short ID '{ShortId}' stored");
        }
    }
}
