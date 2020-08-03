using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class DataProjectViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IClipboard clipboard;

        public DataProjectViewModel(
            DataProject dataProject, 
            IDataApiClient dataApiClient,
            IClipboard clipboard)
        {
            this.dataApiClient = dataApiClient;
            this.clipboard = clipboard;

            Model = dataProject;
            var projectDatabase = new GenericDatabase<DataProject>(dataApiClient);
            ParameterResponsesViewModel = new ProjectParameterResponsesViewModel(dataApiClient, projectDatabase, dataProject);
        }

        public DataProject Model { get; }

        private List<DataProjectUploadInfoViewModel> uploads;
        public List<DataProjectUploadInfoViewModel> Uploads => uploads ?? (uploads = LoadAssociatedData());
        public ProjectParameterResponsesViewModel ParameterResponsesViewModel { get; }

        private List<DataProjectUploadInfoViewModel> LoadAssociatedData()
        {
            return Task.Run(
                () =>
                {
                    var uploadInfos = dataApiClient.GetManyAsync<DataProjectUploadInfo>(
                        $"Data.{nameof(DataProjectUploadInfo.DataProjectId)} = '{Model.Id}'").Result;
                    return uploadInfos
                        .OrderByDescending(x => x.UploadTimestamp)
                        .Select(uploadInfo => new DataProjectUploadInfoViewModel(uploadInfo, dataApiClient, clipboard))
                        .ToList();
                }).Result;
        }
    }
}
