using System;
using System.Collections.Generic;

namespace DataAPI.Tools.Validation
{
    public class TypeValidationRuleBuilder
    {
        private readonly PropertyValidationRuleBuilder propertyValidationRuleBuilder;

        public TypeValidationRuleBuilder()
        {
            propertyValidationRuleBuilder = new PropertyValidationRuleBuilder(this, 5);
        }

        public List<string> Build(Type type, string propertyPath)
        {
            var rules = new List<string>();
            foreach (var property in type.GetProperties())
            {
                var propertyRules = propertyValidationRuleBuilder.Build(property, propertyPath);
                rules.AddRange(propertyRules);
            }
            return rules;
        }
    }
}
