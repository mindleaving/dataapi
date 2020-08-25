using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.Validation
{
    public class RulesetValidationResult
    {
        [JsonConstructor]
        private RulesetValidationResult(
            bool isValid, 
            string ruleset, 
            List<RuleValidationResult> ruleValidationResults)
        {
            Ruleset = ruleset;
            RuleValidationResults = ruleValidationResults;
            IsValid = isValid;
        }

        public static RulesetValidationResult FromRuleValidationResults(string ruleset, List<RuleValidationResult> ruleValidationResults)
            => new RulesetValidationResult(ruleValidationResults.All(x => x.IsValid), ruleset, ruleValidationResults);
        public static RulesetValidationResult Invalid(string ruleset)
            => new RulesetValidationResult(false, ruleset, new List<RuleValidationResult>());
        public static RulesetValidationResult ValidEmpty(string ruleset)
            => new RulesetValidationResult(true, ruleset, new List<RuleValidationResult>());

        public bool IsValid { get; }
        public string Ruleset { get; }
        public List<RuleValidationResult> RuleValidationResults { get; }
    }
}