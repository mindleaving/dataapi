using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class SubmitValidatorBody
    {
        [JsonConstructor]
        public SubmitValidatorBody(
            ValidatorDefinition validatorDefinition)
        {
            ValidatorDefinition = validatorDefinition;
        }

        [Required]
        public ValidatorDefinition ValidatorDefinition { get; }

        public bool SuppressAutoApprove { get; set; }
    }
}
