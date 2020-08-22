using DataAPI.DataStructures.Validation;
using Newtonsoft.Json.Schema;

namespace DataAPI.Service.Validators
{
    public class RulesetValidationDefinitionValidator
    {
        private readonly ValidatorFactory validatorFactory;

        public RulesetValidationDefinitionValidator(ValidatorFactory validatorFactory)
        {
            this.validatorFactory = validatorFactory;
        }

        public RulesetValidationResult Validate(ValidatorDefinition validatorDefinition)
        {
            switch (validatorDefinition.ValidatorType)
            {
                case ValidatorType.TextRules:
                    var textRulesValidator = (TextRulesValidator)validatorFactory.Create(validatorDefinition);
                    return textRulesValidator.IsRulesetSyntaxValid(validatorDefinition.Ruleset);
                case ValidatorType.JsonSchema:
                    try
                    {
                        JSchema.Parse(validatorDefinition.Ruleset);
                        return RulesetValidationResult.ValidEmpty(validatorDefinition.Ruleset);
                    }
                    catch
                    {
                        return RulesetValidationResult.Invalid(validatorDefinition.Ruleset);
                    }
                default:
                    return RulesetValidationResult.Invalid(validatorDefinition.Ruleset);
            }
        }
    }
}
