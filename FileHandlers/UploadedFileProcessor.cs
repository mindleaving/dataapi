using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using FileHandlers.AdditionalInformation;
using FileHandlers.Machines;
using FileHandlers.Objects;
using SharedViewModels.Objects;
using SharedViewModels.ViewModels;

namespace FileHandlers
{
    public class UploadedFileProcessor
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IObjectDatabase<DataBlob> dataBlobDatabase;
        private readonly IObjectDatabase<DataProjectUploadInfo> dataProjectUploadInfoDatabase;
        private readonly IDictionary<string, List<IFileHandler>> fileHandlers;
        private readonly IClipboard clipboard;
        private readonly IAdditionalInformationViewSpawner additionalInformationViewSpawner;

        public UploadedFileProcessor(
            ICollection<IFileHandler> fileHandlers,
            IDataApiClient dataApiClient,
            IObjectDatabase<DataBlob> dataBlobDatabase,
            IObjectDatabase<DataProjectUploadInfo> dataProjectUploadInfoDatabase,
            IClipboard clipboard,
            IAdditionalInformationViewSpawner additionalInformationViewSpawner)
        {
            this.dataApiClient = dataApiClient;
            this.dataBlobDatabase = dataBlobDatabase;
            this.dataProjectUploadInfoDatabase = dataProjectUploadInfoDatabase;
            this.clipboard = clipboard;
            this.additionalInformationViewSpawner = additionalInformationViewSpawner;
            this.fileHandlers = BuildFileHandlerDictionary(fileHandlers);
        }


        public async Task<UploadedFileViewModel> Process(byte[] fileData, string fileName, string dataProjectId, string uploaderInitials)
        {
            try
            {
                return await HandleSingleFile(fileData, fileName, dataProjectId, uploaderInitials);
            }
            catch (Exception e)
            {
                return UploadedFileViewModel.Failed(fileName, e.InnermostException().Message);
            }
        }

        private async Task<UploadedFileViewModel> HandleSingleFile(
            byte[] fileData,
            string fileName,
            string dataProjectId,
            string uploaderInitials)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if(extension == null)
                throw new Exception("File has no extension");
            var matchingFileHandlers = fileHandlers.ContainsKey(extension)
                ? fileHandlers[extension]
                : new List<IFileHandler>();
            var dataBlob = DataBlobBuilder.FromByteArray(fileData, fileName);
            FileHandlingStatus fileStatus;
            if (await dataBlobDatabase.ExistsAsync(dataBlob.Id))
            {
                fileStatus = FileHandlingStatus.AlreadyExists;
            }
            else
            {
                await dataBlobDatabase.StoreAsync(dataBlob);
                fileStatus = FileHandlingStatus.Success;
            }

            Dictionary<string, object> additionalInformation = null;
            if (matchingFileHandlers.Any(x => x.RequiresAdditionalInformation))
            {
                var additionalInformationRequiringFileHandlers = matchingFileHandlers.Where(x => x.RequiresAdditionalInformation).ToList();
                var requestResult = await RequestAdditionalInformation(additionalInformationRequiringFileHandlers, fileName);
                if(!requestResult.IsRequestCompleted)
                    return UploadedFileViewModel.Failed(fileName, "Not enough information provided");
                additionalInformation = requestResult.AdditionalInformationObjects;
            }
            var rawDataReference = DataReference.FromIId(dataBlob);

            var fileViewModels = new List<IDerivedFileDataViewModel>();
            var derivedDataReferences = new List<DataReference>();
            foreach (var matchingFileHandler in matchingFileHandlers)
            {
                var fileHandlerType = matchingFileHandler.GetType().Name;
                var fileHandlerAdditionalInformation = additionalInformation.ContainsKey(fileHandlerType)
                    ? additionalInformation[fileHandlerType]
                    : null;
                var fileHandlerResult =  await matchingFileHandler.Handle(fileData, fileName, dataProjectId, fileHandlerAdditionalInformation);
                fileViewModels.AddRange(fileHandlerResult.ViewModels);
                derivedDataReferences.AddRange(fileHandlerResult.DerivedDataReferences);
            }
            var uploadInfo = new DataProjectUploadInfo(
                uploaderInitials,
                DateTime.UtcNow,
                dataProjectId,
                rawDataReference,
                derivedDataReferences,
                fileName);
            await dataProjectUploadInfoDatabase.StoreAsync(uploadInfo);
            return new UploadedFileViewModel(
                fileName,
                fileStatus,
                string.Empty,
                uploadInfo.Id,
                rawDataReference,
                derivedDataReferences,
                dataApiClient,
                fileViewModels,
                clipboard);
        }

        private async Task<AdditionalInformationRequestResult> RequestAdditionalInformation(
            List<IFileHandler> fileHandlers,
            string fileName)
        {
            var additionalInformationViewModels = fileHandlers.Select(x => x.BuildAdditionalInformationViewModel()).ToList();
            var dialogResult = await additionalInformationViewSpawner.Spawn(fileName, additionalInformationViewModels);
            if(dialogResult != true)
                return new AdditionalInformationRequestResult(false, null);
            var additionalInformations = new Dictionary<string, object>();
            foreach (var additionalInformationViewModel in additionalInformationViewModels)
            {
                var fileHandlerType = additionalInformationViewModel.FileHandlerType;
                additionalInformations.Add(fileHandlerType, additionalInformationViewModel.BuildAdditionalInformationObject());
            }
            return new AdditionalInformationRequestResult(true, additionalInformations);
        }

        private static IDictionary<string, List<IFileHandler>> BuildFileHandlerDictionary(ICollection<IFileHandler> fileHandlers)
        {
            var dictionary = new Dictionary<string, List<IFileHandler>>();
            foreach (var fileHandler in fileHandlers)
            {
                foreach (var supportedExtension in fileHandler.SupportedExtensions)
                {
                    if (!dictionary.ContainsKey(supportedExtension))
                        dictionary.Add(supportedExtension, new List<IFileHandler>());
                    dictionary[supportedExtension].Add(fileHandler);
                }
            }
            return dictionary;
        }
    }
}
