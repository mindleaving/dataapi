using System;
using System.Collections.Generic;
using System.Linq;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace DataAPI.Service.Validators
{
    public class JsonSchemaValidator : IValidator
    {
        public JsonSchemaValidator(ValidatorDefinition definition)
        {
            Definition = definition;
        }

        public ValidatorDefinition Definition { get; }

        public ValidationResult Validate(string json)
        {
            var jToken = JToken.Parse(json);
            if(jToken.Type == JTokenType.Null)
                return ValidationResult.Invalid("Object is null", Definition.Id, Definition.SubmitterEmail);
            var schema = JSchema.Parse(Definition.Ruleset);

            if(!jToken.IsValid(schema, out IList<string> failingRules))
            {
                var aggregatedRules = failingRules.Aggregate((a,b) => a + "\n" + b);
                return ValidationResult.Invalid($"Does not match rule(s):\n{aggregatedRules}", Definition.Id, Definition.SubmitterEmail);
            }
            return ValidationResult.Valid();
        }
    }
}
