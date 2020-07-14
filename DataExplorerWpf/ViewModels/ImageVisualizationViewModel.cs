using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using SharedViewModels.Helpers;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;
using SharedWpfControls.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class ImageVisualizationViewModel : NotifyPropertyChangedBase, IDataVisualizationViewModel
    {
        private readonly IDataApiClient dataApiClient;
        public Type DataType { get; } = typeof(Image);
        private readonly Dictionary<string, Image> images = new Dictionary<string, Image>();

        public ImageVisualizationViewModel(IDataApiClient dataApiClient, List<string> imageIds)
        {
            this.dataApiClient = dataApiClient;
            ImageIds = imageIds;
            ImageCanvas = new ZoomPanCanvasViewModel();
            ExportAllCommand = new AsyncRelayCommand(ExportAll);
            ExportSelectedCommand = new AsyncRelayCommand(ExportSelected, () => SelectedImageId != null);
        }

        public List<string> ImageIds { get; }
        private string selectedImageIdId;
        public string SelectedImageId
        {
            get => selectedImageIdId;
            set
            {
                selectedImageIdId = value;
                if (selectedImageIdId != null)
                {
                    var image = Task.Run(async () => await LoadImage(selectedImageIdId)).Result;

                    if(image != null)
                    {
                        ImageCanvas.SetImage(new System.Windows.Controls.Image
                        {
                            Source = BitmapLoader.FromByteArray(image.Data)
                        });
                    }
                    else
                    {
                        ImageCanvas.ClearCanvas();
                    }
                }
                else
                {
                    ImageCanvas.ClearCanvas();
                }
                OnPropertyChanged();
            }
        }

        private async Task<Image> LoadImage(string imageId)
        {
            if (images.ContainsKey(imageId))
                return images[imageId];
            var image = await dataApiClient.GetAsync<Image>(imageId);
            images.Add(imageId, image);
            return image;
        }

        public IAsyncCommand ExportAllCommand { get; }
        public IAsyncCommand ExportSelectedCommand { get; }
        public ZoomPanCanvasViewModel ImageCanvas { get; }

        private async Task ExportAll()
        {
            var outputDirectoryDialog = new VistaFolderBrowserDialog
            {
                Description = @"Select output folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            if(outputDirectoryDialog.ShowDialog() != true)
                return;

            foreach (var imageId in ImageIds)
            {
                var image = await LoadImage(imageId);
                var outputPath = Path.Combine(outputDirectoryDialog.SelectedPath, $"{imageId}{image.Extension}");
                File.WriteAllBytes(outputPath, image.Data);
            }

            StaticMessageBoxSpawner.Show($"Successfully stored {ImageIds.Count} images");
        }

        private async Task ExportSelected()
        {
            var selectedImage = await LoadImage(SelectedImageId);
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = selectedImage.Extension,
                CheckPathExists = true,
                Title = "Select output file",
                FileName = $"{selectedImage.Id}{selectedImage.Extension}",
                Filter = $"Image (*{selectedImage.Extension})|*{selectedImage.Extension}|All files (*.*)|*.*"
            };
            if(saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                File.WriteAllBytes(saveFileDialog.FileName, selectedImage.Data);
                StaticMessageBoxSpawner.Show("Image successfully stored!");
            }
            catch (Exception e)
            {
                StaticMessageBoxSpawner.Show($"Error: {e.InnermostException().Message}");
            }
        }
    }
}
