using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class UploadedFileViewModel : NotifyPropertyChangedBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IClipboard clipboard;

        public UploadedFileViewModel(
            string fileName, 
            FileHandlingStatus status,
            string statusText,
            string uploadInfoId,
            DataReference rawDataReference,
            List<DataReference> derivedDataReferences,
            IDataApiClient dataApiClient,
            List<IDerivedFileDataViewModel> derivedDataViewModels,
            IClipboard clipboard)
        {
            FileName = fileName;
            Status = status;
            StatusText = statusText;
            UploadInfoId = uploadInfoId;
            RawDataReference = rawDataReference;
            DerivedDataReferences = derivedDataReferences;
            this.dataApiClient = dataApiClient;
            this.clipboard = clipboard;

            DerivedDataViewModels = derivedDataViewModels;
            DeleteFileCommand = new AsyncRelayCommand(DeleteFile);
        }

        public static UploadedFileViewModel Failed(string fileName, string statusText)
        {
            return new UploadedFileViewModel(fileName, FileHandlingStatus.Error, statusText, null, null, null, null, null, null);
        }

        public string FileName { get; }
        public FileHandlingStatus Status { get; }
        public string StatusText { get; }
        public bool CanBeSelected => Status.InSet(FileHandlingStatus.Success, FileHandlingStatus.AlreadyExists);
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
        public string UploadInfoId { get; }
        public ApiConfiguration DataApiConfiguration => dataApiClient.ApiConfiguration;
        public DataReference RawDataReference { get; }
        public bool HasRawDataReference => RawDataReference != null;
        public List<DataReference> DerivedDataReferences { get; }
        public bool HasDerivedDataReferences => DerivedDataReferences != null && DerivedDataReferences.Any();

        public List<IDerivedFileDataViewModel> DerivedDataViewModels { get; }

        public IAsyncCommand DeleteFileCommand { get; }

        public event EventHandler FileDeleted;

        private async Task DeleteFile()
        {
            try
            {
                await DeleteFromDatabase();
                FileDeleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show(e.InnermostException().Message);
            }
        }

        private async Task DeleteFromDatabase()
        {
            if(RawDataReference != null)
                await dataApiClient.DeleteAsync(RawDataReference.DataType, RawDataReference.Id);
            if(DerivedDataReferences != null)
            {
                foreach (var derivedDataReference in DerivedDataReferences)
                {
                    await dataApiClient.DeleteAsync(derivedDataReference.DataType, derivedDataReference.Id);
                }
            }
        }
    }
}
