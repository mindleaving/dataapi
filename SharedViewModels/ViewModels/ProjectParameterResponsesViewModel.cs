using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.Exceptions;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class ProjectParameterResponsesViewModel : NotifyPropertyChangedBase
    {
        private readonly IObjectDatabase<DataProject> dataProjectDatabase;

        public ProjectParameterResponsesViewModel(
            IDataApiClient dataApiClient,
            
            IObjectDatabase<DataProject> dataProjectDatabase, 
            DataProject dataProject)
        {
            this.dataProjectDatabase = dataProjectDatabase;
            DataProject = dataProject;
            var valueViewModelFactory = new ParameterValueViewModelFactory(dataApiClient);
            ParameterResponses = dataProject.Protocol.Parameters
                .Select(parameter =>
                {
                    var response = dataProject.ParameterResponse.ContainsKey(parameter.Name)
                        ? dataProject.ParameterResponse[parameter.Name]
                        : null;
                    return new ProtocolParameterResponseViewModel(parameter, valueViewModelFactory, response);
                })
                .ToList();
            ParameterResponses.ForEach(vm => vm.PropertyChanged += ParameterResponse_PropertyChanged);
            ShowParameters = !IsProtocolParametersCompleted;

            SaveCommand = new AsyncRelayCommand(Save, () => IsProtocolParametersCompleted);
        }

        private void ParameterResponse_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsProtocolParametersCompleted));
            if (!IsProtocolParametersCompleted)
                ShowParameters = true;
        }

        public bool IsProtocolParametersCompleted => ParameterResponses
            .All(response => !response.IsMandatory || !string.IsNullOrEmpty(response.Value));

        /// <summary>
        /// Hack for opening/closing expander. This should not be part of view model,
        /// but logic is a little strange and harder to implement in view
        /// </summary>
        private bool showParameters;
        public bool ShowParameters
        {
            get => showParameters;
            set
            {
                showParameters = !IsProtocolParametersCompleted || value;
                OnPropertyChanged();
            }
        }

        public DataProject DataProject { get; }
        public List<ProtocolParameterResponseViewModel> ParameterResponses { get; }

        public IAsyncCommand SaveCommand { get; }

        private async Task Save()
        {
            foreach (var response in ParameterResponses)
            {
                DataProject.ParameterResponse[response.Name] = response.Value;
            }

            try
            {
                await dataProjectDatabase.StoreAsync(DataProject);
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == HttpStatusCode.Unauthorized)
                    StaticMessageBoxSpawner.Show("You are not authorized to change data projects. Please contact Inno-IT.");
            }
            if (IsProtocolParametersCompleted)
                ShowParameters = false;
        }
    }
}
