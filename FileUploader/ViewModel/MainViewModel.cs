using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AutoCompleteMatchers;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using FileHandlers;
using FileUploader.Views;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;
using SharedWpfControls.Views;

namespace FileUploader.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly IClosable mainWindow;
        private readonly IDataApiClient dataApiClient;
        private readonly UploadedFileProcessor uploadedFileProcessor;
        private readonly IObjectDatabase<DataSet> dataSetDatabase;
        private readonly IObjectDatabase<DataTag> tagDatabase;
        private readonly IObjectDatabase<DataProject> projectDatabase;
        private readonly IObjectDatabase<DataCollectionProtocol> protocolDatabase;
        private readonly IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase;

        public MainViewModel(
            IClosable mainWindow,
            IDataApiClient dataApiClient,
            UploadedFileProcessor uploadedFileProcessor,
            IObjectDatabase<DataSet> dataSetDatabase, 
            IObjectDatabase<DataTag> tagDatabase, 
            IObjectDatabase<DataProject> projectDatabase,
            IObjectDatabase<DataCollectionProtocol> protocolDatabase,
            IObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase)
        {
            this.mainWindow = mainWindow;
            this.dataApiClient = dataApiClient;
            this.uploadedFileProcessor = uploadedFileProcessor;
            this.dataSetDatabase = dataSetDatabase;
            this.tagDatabase = tagDatabase;
            this.projectDatabase = projectDatabase;
            this.protocolDatabase = protocolDatabase;
            this.protocolParameterDatabase = protocolParameterDatabase;

            var passwordBoxSpawner = new WpfPasswordBoxSpawner();
            var loginViewSpawner = new WpfWindowSpawner<LoginViewModel>(vm => new LoginWindow { ViewModel = vm });
            var changePasswordViewSpawner = new WpfWindowSpawner<ChangePasswordViewModel>(vm => new ChangePasswordWindow { ViewModel = vm });
            UserSwitchViewModel = new UserSwitchViewModel(
                dataApiClient,
                LoginSuccessful, 
                LoginFailed,
                passwordBoxSpawner,
                loginViewSpawner,
                changePasswordViewSpawner);
            ProjectAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataProject>(
                x => x.Id,
                projectDatabase,
                new DataProjectAutoCompleteMatcher());
            ProjectAutoCompleteViewModel.PropertyChanged += ProjectAutoCompleteViewModel_PropertyChanged;
            DataSetAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataSet>(
                x => x.Id,
                dataSetDatabase,
                new DataSetAutoCompleteMatcher(),
                allowNewValue: true,
                objectBuilder: id => new DataSet(id));
            CreateNewProjectCommand = new RelayCommand(CreateNewProject);
            AddSelectedFilesToDataSetCommand = new AsyncRelayCommand(AddSelectedFilesToDataSet, CanAddSelectedFilesToDataSet);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
            DeselectAllCommand = new RelayCommand(DeselectAll, CanDeselectAll);
            OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);
        }

        public UserSwitchViewModel UserSwitchViewModel { get; }

        public AutoCompleteTextBoxViewModel<DataProject> ProjectAutoCompleteViewModel { get; }
        private bool isProjectSelected;
        public bool IsProjectSelected
        {
            get => isProjectSelected;
            private set
            {
                isProjectSelected = value;
                OnPropertyChanged();
            }
        }
        private bool isProtocolParametersCompleted;
        public bool IsProtocolParametersCompleted
        {
            get => isProtocolParametersCompleted;
            private set
            {
                isProtocolParametersCompleted = value;
                OnPropertyChanged();
            }
        }
        private ProjectParameterResponsesViewModel parameterResponsesViewModel;
        public ProjectParameterResponsesViewModel ParameterResponsesViewModel
        {
            get => parameterResponsesViewModel;
            private set
            {
                parameterResponsesViewModel = value;
                OnPropertyChanged();
            }
        }
        public AutoCompleteTextBoxViewModel<DataSet> DataSetAutoCompleteViewModel { get; }
        public ObservableCollection<UploadedFileViewModel> FileList { get; } = new ObservableCollection<UploadedFileViewModel>();

        public ICommand CreateNewProjectCommand { get; }
        public IAsyncCommand AddSelectedFilesToDataSetCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand OpenAboutWindowCommand { get; }

        public void LoginSuccessful()
        {
        }

        private void LoginFailed()
        {
            mainWindow.Close(false);
        }

        private async void CreateNewProject()
        {
            var createProtocolViewSpawner = new WpfWindowSpawner<CreateProtocolViewModel>(vm => new CreateProtocolWindow { ViewModel = vm });
            var createProjectViewModel = new CreateProjectViewModel(
                dataApiClient,
                
                projectDatabase,
                protocolDatabase,
                protocolParameterDatabase,
                createProtocolViewSpawner);
            var createProjectWindow = new CreateProjectWindow
            {
                ViewModel = createProjectViewModel
            };
            if(createProjectWindow.ShowDialog() != true)
                return;
            ProjectAutoCompleteViewModel.SelectedObject = await projectDatabase.GetFromIdAsync(createProjectViewModel.ProjectId);
        }

        public async Task HandleFileDrop(string[] filePaths)
        {
            var selectedProject = ProjectAutoCompleteViewModel.SelectedObject;
            if (selectedProject == null)
            {
                StaticMessageBoxSpawner.Show("No data project selected");
                return;
            }

            foreach (var filePath in filePaths)
            {
                var fileData = File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath);
                var uploaderInitials = dataApiClient.LoggedInUsername;
                var fileViewModel = await uploadedFileProcessor.Process(fileData, fileName, selectedProject.Id, uploaderInitials);
                FileList.Insert(0, fileViewModel);
                fileViewModel.FileDeleted += DeleteFile;
            }
        }

        private bool CanAddSelectedFilesToDataSet()
        {
            return DataSetAutoCompleteViewModel.SelectedObject != null
                && FileList.Any(fileViewModel => fileViewModel.IsSelected);
        }

        private async Task AddSelectedFilesToDataSet()
        {
            var dataSet = DataSetAutoCompleteViewModel.SelectedObject;
            var selectedFiles = FileList
                .Where(fileViewModel => fileViewModel.IsSelected)
                .SelectMany(fileViewModel => fileViewModel.DerivedDataReferences.Concat(new []{fileViewModel.RawDataReference}))
                .ToList();
            if(selectedFiles.Any(dataReference => dataReference == null))
                throw new Exception("Files that can be selected must return a non-null data reference");
            foreach (var dataReference in selectedFiles)
            {
                await tagDatabase.StoreAsync(new DataTag(dataReference, dataSet.Id));
            }
            try
            {
                await dataSetDatabase.StoreAsync(dataSet);
                StaticMessageBoxSpawner.Show($"Data set '{dataSet.Id}' stored");
                DataSetAutoCompleteViewModel.SelectedObject = dataSet;
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Error: {e.InnermostException().Message}");
            }
        }

        private void DeleteFile(object sender, EventArgs e)
        {
            var fileViewModel = sender as UploadedFileViewModel;
            if(fileViewModel == null)
                return;
            Application.Current.Dispatcher.BeginInvoke((Action) (() => FileList.Remove(fileViewModel)));
        }

        private bool CanSelectAll()
        {
            return FileList
                       .Where(fileViewModel => fileViewModel.CanBeSelected)
                       .Count(fileViewModel => !fileViewModel.IsSelected) > 0;
        }

        private void SelectAll()
        {
            FileList
                .Where(fileViewModel => fileViewModel.CanBeSelected)
                .ForEach(fileViewModel => fileViewModel.IsSelected = true);
        }

        private bool CanDeselectAll()
        {
            return FileList
                       .Where(fileViewModel => fileViewModel.CanBeSelected)
                       .Count(fileViewModel => fileViewModel.IsSelected) > 0;
        }

        private void DeselectAll()
        {
            FileList
                .Where(fileViewModel => fileViewModel.CanBeSelected)
                .ForEach(fileViewModel => fileViewModel.IsSelected = false);
        }

        private void ProjectAutoCompleteViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(ProjectAutoCompleteViewModel.SelectedObject))
                return;
            var selectedProject = ProjectAutoCompleteViewModel.SelectedObject;
            IsProjectSelected = selectedProject != null;
            if (IsProjectSelected)
            {
                LoadProject(selectedProject);
            }
            else
            {
                ParameterResponsesViewModel = null;
                IsProtocolParametersCompleted = false;
            }
        }

        private void LoadProject(DataProject dataProject)
        {
            ParameterResponsesViewModel = new ProjectParameterResponsesViewModel(dataApiClient, projectDatabase, dataProject);
            ParameterResponsesViewModel.PropertyChanged += ParameterResponsesViewModel_PropertyChanged;
            IsProtocolParametersCompleted = ParameterResponsesViewModel.IsProtocolParametersCompleted;
        }

        private void ParameterResponsesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(!sender.Equals(ParameterResponsesViewModel))
                return;
            IsProtocolParametersCompleted = ParameterResponsesViewModel.IsProtocolParametersCompleted;
        }

        private void OpenAboutWindow()
        {
            new AboutWindow().ShowDialog();
        }
    }
}
