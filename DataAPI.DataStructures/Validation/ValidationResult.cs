using System;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.Validation
{
    public class ValidationResult
    {
        [JsonConstructor]
        private ValidationResult(bool isValid, 
            string errorDescription = null, 
            string rejectingValidatorId = null, 
            string rejectingValidatorEmail = null)
        {
            IsValid = isValid;
            if (!isValid)
            {
                ErrorDescription = errorDescription ?? throw new ArgumentNullException(nameof(errorDescription));
                RejectingValidatorId = rejectingValidatorId ?? throw new ArgumentNullException(nameof(rejectingValidatorId));
                RejectingValidatorEmail = rejectingValidatorEmail ?? throw new ArgumentNullException(nameof(rejectingValidatorEmail));
            }
        }
        public static ValidationResult Valid() => new ValidationResult(true);
        public static ValidationResult Invalid(string errorDescription, string validatorId, string validatorEmail)
            => new ValidationResult(false, errorDescription, validatorId, validatorEmail);

        public bool IsValid { get; }
        public string ErrorDescription { get; }
        public string RejectingValidatorId { get; }
        public string RejectingValidatorEmail { get; }
    }
}
