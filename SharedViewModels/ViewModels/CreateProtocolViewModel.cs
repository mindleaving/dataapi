using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class CreateProtocolViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly IObjectDatabase<DataCollectionProtocol> protocolDatabase;
        private readonly IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase;

        public CreateProtocolViewModel(
            IDataApiClient dataApiClient,
            IObjectDatabase<DataCollectionProtocol> protocolDatabase,
            IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase,
            DataCollectionProtocol dataCollectionProtocol = null)
        {
            this.dataApiClient = dataApiClient;
            
            this.protocolDatabase = protocolDatabase;
            this.protocolParameterDatabase = protocolParameterDatabase;
            if (dataCollectionProtocol != null)
            {
                IsNewProtocol = false;
                ProtocolId = dataCollectionProtocol.Id;
                Parameters = new ObservableCollection<ProtocolParameterViewModel>(
                    dataCollectionProtocol.Parameters.Select(
                        x => new ProtocolParameterViewModel(
                            dataApiClient,
                            protocolParameterDatabase,
                            x)));
                DataPlaceholders = new ObservableCollection<DataPlaceholdersViewModel>(
                    dataCollectionProtocol.ExpectedData.Select(x => new DataPlaceholdersViewModel(x)));
            }
            else
            {
                IsNewProtocol = true;
                Parameters = new ObservableCollection<ProtocolParameterViewModel>
                {
                    new ProtocolParameterViewModel(dataApiClient, protocolParameterDatabase)
                };
                DataPlaceholders = new ObservableCollection<DataPlaceholdersViewModel>
                {
                    new DataPlaceholdersViewModel()
                };
            }
            AddParameterCommand = new RelayCommand(AddParameter);
            DeleteParameterCommand = new RelayCommand(DeleteParameter, () => SelectedParameter != null);
            AddDataPlaceholderCommand = new RelayCommand(AddDataPlaceholder);
            DeleteDataPlaceholderCommand = new RelayCommand(DeleteDataPlaceholder, () => SelectedDataPlaceholder != null);
            SaveCommand = new AsyncRelayCommand<IClosable>(Save, CanSave);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
        }

        public bool IsNewProtocol { get; }

        private string protocolId;
        public string ProtocolId
        {
            get => protocolId;
            set
            {
                protocolId = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProtocolParameterViewModel> Parameters { get; }
        private ProtocolParameterViewModel selectedParameter;
        public ProtocolParameterViewModel SelectedParameter
        {
            get => selectedParameter;
            set
            {
                selectedParameter = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DataPlaceholdersViewModel> DataPlaceholders { get; }
        private DataPlaceholdersViewModel selectedDataPlaceholder;
        public DataPlaceholdersViewModel SelectedDataPlaceholder
        {
            get => selectedDataPlaceholder;
            set
            {
                selectedDataPlaceholder = value;
                OnPropertyChanged();
            }
        }

        private bool isSaving;
        public bool IsSaving
        {
            get => isSaving;
            private set
            {
                isSaving = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddParameterCommand { get; }
        public ICommand DeleteParameterCommand { get; }
        public ICommand AddDataPlaceholderCommand { get; }
        public ICommand DeleteDataPlaceholderCommand { get; }
        public IAsyncCommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void AddParameter()
        {
            Parameters.Add(new ProtocolParameterViewModel(dataApiClient, protocolParameterDatabase));
        }

        private void DeleteParameter()
        {
            Parameters.Remove(SelectedParameter);
        }

        private void AddDataPlaceholder()
        {
            DataPlaceholders.Add(new DataPlaceholdersViewModel());
        }

        private void DeleteDataPlaceholder()
        {
            DataPlaceholders.Remove(SelectedDataPlaceholder);
        }

        private bool CanSave(IClosable closable)
        {
            return !string.IsNullOrWhiteSpace(ProtocolId)
                && Parameters.All(x => x.IsValid() || x.IsEmpty())
                && DataPlaceholders.All(x => x.IsValid() || x.IsEmpty());
        }

        private async Task Save(IClosable closable)
        {
            IsSaving = true;
            var cleanedProtocolId = ProtocolId.Trim();
            if (IsNewProtocol)
            {
                var protocolExists = await protocolDatabase.ExistsAsync(cleanedProtocolId);
                if (protocolExists)
                {
                    StaticMessageBoxSpawner.Show($"Project with ID '{cleanedProtocolId}' already exists");
                    IsSaving = false;
                    return;
                }
            }

            var parameterModels = Parameters
                .Where(x => !x.IsEmpty())
                .Select(x => x.Build())
                .ToList();
            var dataPlaceholderModels = DataPlaceholders
                .Where(x => !x.IsEmpty())
                .Select(x => x.Build())
                .ToList();
            var protocol = new DataCollectionProtocol(
                cleanedProtocolId, 
                parameterModels,
                dataPlaceholderModels);
            await protocolDatabase.StoreAsync(protocol);
            foreach (var parameter in parameterModels)
            {
                await protocolParameterDatabase.StoreAsync(parameter);
            }
            IsSaving = false;
            closable.Close(true);
        }
    }
}
