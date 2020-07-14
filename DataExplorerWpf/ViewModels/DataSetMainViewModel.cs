using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCompleteMatchers;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataSetMainViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IReadonlyObjectDatabase<DataSet> dataSetDatabase;
        private readonly IReadonlyObjectDatabase<DataProject> projectDatabase;
        private readonly IClipboard clipboard;

        public DataSetMainViewModel(
            IDataApiClient dataApiClient,
            IReadonlyObjectDatabase<DataSet> dataSetDatabase,
            IReadonlyObjectDatabase<DataProject> projectDatabase,
            IClipboard clipboard)
        {
            this.dataApiClient = dataApiClient;
            this.dataSetDatabase = dataSetDatabase;
            this.projectDatabase = projectDatabase;
            this.clipboard = clipboard;
            DataSetAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataSet>(
                x => x.Id,
                dataSetDatabase,
                new DataSetAutoCompleteMatcher());
            DataSetAutoCompleteViewModel.PropertyChanged += DataSetAutoCompleteViewModel_PropertyChanged;
            ProjectAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataProject>(
                x => x.Id,
                projectDatabase,
                new DataProjectAutoCompleteMatcher());
            ProjectAutoCompleteViewModel.PropertyChanged += ProjectAutoCompleteViewModel_PropertyChanged;

            LoadDataSetListCommand = new AsyncRelayCommand(LoadDataSetList);
            LoadDataProjectListCommand = new AsyncRelayCommand(LoadDataProjectList);
        }

        private void DataSetAutoCompleteViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(AutoCompleteTextBoxViewModel<DataSet>.SelectedObject))
                return;
            SelectedDataSet = DataSetAutoCompleteViewModel.SelectedObject;
        }

        private void ProjectAutoCompleteViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(AutoCompleteTextBoxViewModel<DataProject>.SelectedObject))
                return;
            SelectedDataProject = ProjectAutoCompleteViewModel.SelectedObject;
        }

        public AutoCompleteTextBoxViewModel<DataSet> DataSetAutoCompleteViewModel { get; }
        private bool isDataSetsLoaded;
        public bool IsDataSetsLoaded
        {
            get => isDataSetsLoaded;
            private set
            {
                isDataSetsLoaded = value;
                OnPropertyChanged();
            }
        }
        private List<DataSet> dataSets;
        public List<DataSet> DataSets
        {
            get => dataSets;
            private set
            {
                dataSets = value;
                IsDataSetsLoaded = dataSets != null;
                OnPropertyChanged();
            }
        }
        private DataSet selectedDataSet;
        public DataSet SelectedDataSet
        {
            get => selectedDataSet;
            set
            {
                selectedDataSet = value;
                SelectedDataSetViewModel = selectedDataSet != null
                    ? new DataSetViewModel(selectedDataSet, dataApiClient)
                    : null;
                OnPropertyChanged();
            }
        }
        private DataSetViewModel selectedDataSetViewModel;
        public DataSetViewModel SelectedDataSetViewModel
        {
            get => selectedDataSetViewModel;
            private set
            {
                selectedDataSetViewModel = value;
                OnPropertyChanged();
            }
        }

        public AutoCompleteTextBoxViewModel<DataProject> ProjectAutoCompleteViewModel { get; }
        private bool isDataProjectsLoaded;
        public bool IsDataProjectsLoaded
        {
            get => isDataProjectsLoaded;
            private set
            {
                isDataProjectsLoaded = value;
                OnPropertyChanged();
            }
        }
        private List<DataProject> dataProjects;
        public List<DataProject> DataProjects
        {
            get => dataProjects;
            private set
            {
                dataProjects = value;
                IsDataProjectsLoaded = dataProjects != null;
                OnPropertyChanged();
            }
        }
        private DataProject selectedDataProject;
        public DataProject SelectedDataProject
        {
            get => selectedDataProject;
            set
            {
                selectedDataProject = value;
                SelectedDataProjectViewModel = selectedDataProject != null
                    ? new DataProjectViewModel(selectedDataProject, dataApiClient, clipboard)
                    : null;
                OnPropertyChanged();
            }
        }
        private DataProjectViewModel selectedDataProjectViewModel;
        public DataProjectViewModel SelectedDataProjectViewModel
        {
            get => selectedDataProjectViewModel;
            private set
            {
                selectedDataProjectViewModel = value;
                OnPropertyChanged();
            }
        }

        public IAsyncCommand LoadDataSetListCommand { get; }
        public IAsyncCommand LoadDataProjectListCommand { get; }

        private async Task LoadDataSetList()
        {
            DataSets = (await dataSetDatabase.GetAllAsync()).ToList();
        }

        private async Task LoadDataProjectList()
        {
            DataProjects = (await projectDatabase.GetAllAsync()).ToList();
        }
    }
}
