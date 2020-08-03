using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataManagement;
using FileHandlers.AdditionalInformation;
using FileHandlers.Objects;
using SharedViewModels.ViewModels;

namespace FileHandlers
{
    public class ImageFileHandler : IFileHandler
    {
        private readonly IObjectDatabase<Image> imageDatabase;

        public ImageFileHandler(IObjectDatabase<Image> imageDatabase)
        {
            this.imageDatabase = imageDatabase;
        }

        public string[] SupportedExtensions { get; } = {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".tif",
            ".tiff"
        };

        public bool RequiresAdditionalInformation { get; } = false;
        public IAdditionalInformationViewModel BuildAdditionalInformationViewModel() 
            => throw new InvalidOperationException("Additional information view model is only available if additional information is required");

        public async Task<FileHandlerResult> Handle(byte[] fileData, string fileName, string dataProjectId, object additionalInformation = null)
        {
            var imageId = IdGenerator.Sha1HashFromByteArray(fileData);
            var derivedDataReferences = new List<DataReference>
            {
                new DataReference(nameof(Image), imageId)
            };
            if (!await imageDatabase.ExistsAsync(imageId))
            {
                var image = new Image(imageId, fileData, Path.GetExtension(fileName), fileName);
                await imageDatabase.StoreAsync(image);
            }
            return new FileHandlerResult(derivedDataReferences, new List<IDerivedFileDataViewModel>());
        }
    }
}
