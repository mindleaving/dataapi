using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace DataAPI.DataStructures.PostBodies
{
    public class SubmitDataBody
    {
        [JsonConstructor]
        public SubmitDataBody(string dataType, object data, bool overwrite, string id = null)
        {
            DataType = dataType;
            Data = data;
            Overwrite = overwrite;
            Id = id;
        }

        [Required]
        public string DataType { get; }

        [TypescriptIsOptional]
        public string Id { get; }

        [Required]
        public bool Overwrite { get; }

        [Required]
        public object Data { get; }
    }
}