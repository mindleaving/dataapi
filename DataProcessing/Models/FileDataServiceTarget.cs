using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.Distribution;
using Newtonsoft.Json;

namespace DataProcessing.Models
{
    public class FileDataServiceTarget : IDataServiceTarget
    {
        [JsonConstructor]
        public FileDataServiceTarget(
            string id,
            string directory,
            DataServiceFileFormat fileFormat)
        {
            Id = id;
            Directory = directory;
            FileFormat = fileFormat;
        }

        public string Id { get; }
        public DataServiceTargetType Type { get; } = DataServiceTargetType.File;
        public string Directory { get; }
        public DataServiceFileFormat FileFormat { get; }

        public override string ToString()
        {
            return $"{Type} {Directory} {FileFormat}";
        }
    }
}