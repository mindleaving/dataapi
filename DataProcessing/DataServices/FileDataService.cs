using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DataAPI.DataStructures.Constants;
using DataProcessing.Models;
using Newtonsoft.Json.Linq;

namespace DataProcessing.DataServices
{
    public class FileDataService : IDataService
    {
        private readonly FileDataServiceTarget target;

        public FileDataService(FileDataServiceTarget target)
        {
            this.target = target;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task TransferAsync(JObject jObject, List<DataServiceDefinition.Field> fields, string id)
        {
            var filePath = BuildFilePath(id);
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            var filePath = BuildFilePath(id);
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public Task<bool> ObjectExistsAsync(string id)
        {
            var filePath = BuildFilePath(id);
            return Task.FromResult(File.Exists(filePath));
        }

        private string BuildFilePath(string id)
        {
            var extension = DetermineExtension(target.FileFormat);
            return Path.Combine(target.Directory, id + extension);
        }

        private string DetermineExtension(DataServiceFileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case DataServiceFileFormat.Csv:
                    return ".csv";
                case DataServiceFileFormat.Xlsx:
                    return ".xlsx";
                default:
                    return string.Empty;
            }
        }
    }
}