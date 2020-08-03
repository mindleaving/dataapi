using DataAPI.DataStructures.Validation;

namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetValidatorResourceDescription : IDataResourceDescription
    {
        public GetValidatorResourceDescription(ValidatorDefinition validatorDefinition)
        {
            ValidatorDefinition = validatorDefinition;
        }

        public ResourceType Type => ResourceType.GetValidator;
        public ValidatorDefinition ValidatorDefinition { get; }
        public string DataType => ValidatorDefinition.DataType;
    }
}