using System;
using System.IO;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.Exceptions;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using SharedViewModels.Helpers;

namespace DataExplorerWpf.ViewModels
{
    public static class DataDownloadHelpers
    {
        public static async Task Download(DataReference dataReference, IDataApiClient dataApiClient)
        {
            try
            {
                var json = await dataApiClient.GetAsync(dataReference.DataType, dataReference.Id);
                if (dataReference.DataType == nameof(DataBlob))
                {
                    var dataBlob = JsonConvert.DeserializeObject<DataBlob>(json);
                    var extension = dataBlob.Filename != null
                        ? Path.GetExtension(dataBlob.Filename)
                        : ".bin";
                    var fileDialog = new VistaSaveFileDialog
                    {
                        AddExtension = true,
                        DefaultExt = extension,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        Filter = @"All files (*.*)|*.*",
                        FileName = dataBlob.Filename
                    };
                    if (fileDialog.ShowDialog() != true)
                        return;
                    var outputFilePath = fileDialog.FileName;

                    File.WriteAllBytes(outputFilePath, dataBlob.Data);
                }
                else if (dataReference.DataType == nameof(Image))
                {
                    var image = JsonConvert.DeserializeObject<Image>(json);
                    var extension = image.Extension;
                    var fileDialog = new VistaSaveFileDialog
                    {
                        AddExtension = true,
                        DefaultExt = extension,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        Filter = @"All files (*.*)|*.*",
                        FileName = image.Filename ?? $"{image.Id}{extension}"
                    };
                    if (fileDialog.ShowDialog() != true)
                        return;
                    var outputFilePath = fileDialog.FileName;

                    File.WriteAllBytes(outputFilePath, image.Data);
                }
                else
                {
                    var fileDialog = new VistaSaveFileDialog
                    {
                        AddExtension = true,
                        DefaultExt = ".json",
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        Filter = @"JSON (*.json)|*.json|All files (*.*)|*.*",
                        FileName = $"{dataReference.DataType}_{dataReference.Id}.json"
                    };
                    if (fileDialog.ShowDialog() != true)
                        return;

                    var outputFilePath = fileDialog.FileName;
                    File.WriteAllText(outputFilePath, json);
                }
            }
            catch (Exception e)
            {
                var errorText = e.InnermostException().Message;
                if (string.IsNullOrEmpty(errorText) && e is ApiException apiException)
                    errorText = apiException.StatusCode.ToString();
                StaticMessageBoxSpawner.Show($"Could not download data: {errorText}");
            }
        }
    }
}
