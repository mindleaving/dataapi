using DataAPI.DataStructures.Validation;

namespace DataAPI.Service.Validators
{
    public interface IValidator
    {
        ValidatorDefinition Definition { get; }
        ValidationResult Validate(string json);
    }
}