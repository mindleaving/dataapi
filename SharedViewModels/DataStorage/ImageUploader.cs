using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Exceptions;

namespace SharedViewModels.DataStorage
{
    public class ImageUploader
    {
        private readonly IObjectDatabase<Image> imageDatabase;

        public ImageUploader(IObjectDatabase<Image> imageDatabase)
        {
            this.imageDatabase = imageDatabase;
        }

        public async Task UploadImages(IEnumerable<string> imageFiles)
        {
            foreach (var imageFile in imageFiles)
            {
                await UploadImage(imageFile);
            }
        }

        public async Task UploadImage(string imagePath)
        {
            if(!File.Exists(imagePath))
                throw new FileNotFoundException("Image not found", imagePath);
            var image = new Image(
                IdGenerator.Sha1HashFromFile(imagePath), 
                File.ReadAllBytes(imagePath),
                Path.GetExtension(imagePath));
            await UploadImage(image);
        }

        public async Task UploadImage(Image image)
        {
            try
            {
                var exists = await imageDatabase.ExistsAsync(image.Id);
                if (!exists)
                    await imageDatabase.StoreAsync(image);
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode != HttpStatusCode.Conflict) // Ignore existing image
                    throw new Exception($"Could not upload image: {apiException.Message} (status code {apiException.StatusCode})");
            }
        }
    }
}
