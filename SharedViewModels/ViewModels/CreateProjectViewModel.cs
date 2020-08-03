using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoCompleteMatchers;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class CreateProjectViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly IObjectDatabase<DataProject> projectDatabase;
        private readonly IObjectDatabase<DataCollectionProtocol> protocolDatabase;
        private readonly IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase;
        private readonly IViewSpawner<CreateProtocolViewModel> createProtocolViewSpawner;

        public CreateProjectViewModel(
            IDataApiClient dataApiClient,
            
            IObjectDatabase<DataProject> projectDatabase,
            IObjectDatabase<DataCollectionProtocol> protocolDatabase,
            IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase,
            IViewSpawner<CreateProtocolViewModel> createProtocolViewSpawner)
        {
            this.dataApiClient = dataApiClient;
            
            this.projectDatabase = projectDatabase;
            this.protocolDatabase = protocolDatabase;
            this.protocolParameterDatabase = protocolParameterDatabase;
            this.createProtocolViewSpawner = createProtocolViewSpawner;
            ProtocolsAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataCollectionProtocol>(
                x => x.Id,
                protocolDatabase,
                new DataCollectionProtocolAutoCompleteMatcher());
            CreateNewProtocolCommand = new RelayCommand(CreateNewProtocol);
            EditProtocolCommand = new RelayCommand(EditProtocol, () => ProtocolsAutoCompleteViewModel.SelectedObject != null);
            SaveCommand = new AsyncRelayCommand<IClosable>(Save, CanSave);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
        }

        private string projectId;
        public string ProjectId
        {
            get => projectId;
            set
            {
                projectId = value;
                OnPropertyChanged();
            }
        }

        public List<string> IdSourceSystems { get; } = new List<string>();
        private string selectedIdSourceSystem;
        public string SelectedIdSourceSystem
        {
            get => selectedIdSourceSystem;
            set
            {
                selectedIdSourceSystem = value;
                OnPropertyChanged();
            }
        }

        public AutoCompleteTextBoxViewModel<DataCollectionProtocol> ProtocolsAutoCompleteViewModel { get; }

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

        public ICommand CreateNewProtocolCommand { get; }
        public ICommand EditProtocolCommand { get; }
        public IAsyncCommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private async void CreateNewProtocol()
        {
            var createProtocolViewModel = new CreateProtocolViewModel(dataApiClient, protocolDatabase, protocolParameterDatabase);
            if(createProtocolViewSpawner.SpawnBlocking(createProtocolViewModel) != true)
                return;
            ProtocolsAutoCompleteViewModel.SelectedObject = await protocolDatabase.GetFromIdAsync(createProtocolViewModel.ProtocolId);
        }

        private async void EditProtocol()
        {
            var createProtocolViewModel = new CreateProtocolViewModel(
                dataApiClient,
                
                protocolDatabase,
                protocolParameterDatabase,
                ProtocolsAutoCompleteViewModel.SelectedObject);
            if(createProtocolViewSpawner.SpawnBlocking(createProtocolViewModel) != true)
                return;
            ProtocolsAutoCompleteViewModel.SelectedObject = await protocolDatabase.GetFromIdAsync(createProtocolViewModel.ProtocolId);
        }

        private bool CanSave(IClosable closable)
        {
            return !string.IsNullOrWhiteSpace(ProjectId)
                   && ProtocolsAutoCompleteViewModel.SelectedObject != null;
        }

        private async Task Save(IClosable closable)
        {
            IsSaving = true;
            var cleanedProjectId = ProjectId.Trim();
            var projectExists = await projectDatabase.ExistsAsync(cleanedProjectId);
            if (projectExists)
            {
                StaticMessageBoxSpawner.Show($"Project with ID '{cleanedProjectId}' already exists");
                IsSaving = false;
                return;
            }
            var project = new DataProject(
                cleanedProjectId,
                SelectedIdSourceSystem,
                ProtocolsAutoCompleteViewModel.SelectedObject,
                new Dictionary<string, string>());
            await projectDatabase.StoreAsync(project);
            IsSaving = false;
            closable.Close(true);
        }
    }
}
