using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class ApplyValidatorBody
    {
        [JsonConstructor]
        public ApplyValidatorBody(string dataType, object data, string validatorId = null)
        {
            DataType = dataType;
            Data = data;
            ValidatorId = validatorId;
        }

        [Required]
        public string DataType { get; }

        [Required]
        public string ValidatorId { get; }

        [Required]
        public object Data { get; }
    }
}
