using Newtonsoft.Json;

namespace DataAPI.DataStructures.Validation
{
    public class RuleValidationResult
    {
        [JsonConstructor]
        private RuleValidationResult(
            bool isValid, 
            string rule, 
            string errorDescription)
        {
            IsValid = isValid;
            Rule = rule;
            ErrorDescription = errorDescription;
        }
        public static RuleValidationResult Valid(string rule) => new RuleValidationResult(true, rule, null);
        public static RuleValidationResult Invalid(string rule, string errorDescription) => new RuleValidationResult(false, rule, errorDescription);

        public bool IsValid { get; }
        public string Rule { get; }
        public string ErrorDescription { get; }
    }
}