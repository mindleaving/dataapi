using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public class TextRulesValidator : IValidator
    {
        private readonly TextRuleEvaluator textRuleEvaluator;

        public TextRulesValidator(ValidatorDefinition definition, TextRuleEvaluator textRuleEvaluator)
        {
            Definition = definition;
            this.textRuleEvaluator = textRuleEvaluator;
        }

        public ValidatorDefinition Definition { get; }

        public ValidationResult Validate(string json)
        {
            var jToken = JToken.Parse(json);
            if(jToken.Type == JTokenType.Null)
                return ValidationResult.Invalid("Object is null", Definition.Id, Definition.SubmitterEmail);
            var rules = TextRulesetParser.ExtractRules(Definition.Ruleset);
            var failingRules = new List<string>();
            foreach (var rule in rules)
            {
                try
                {
                    if (IsConditional(rule, out var validationRule, out var predicateRule))
                    {
                        if (textRuleEvaluator.MatchesRule(jToken, predicateRule))
                        {
                            if (!textRuleEvaluator.MatchesRule(jToken, validationRule))
                            {
                                failingRules.Add(rule);
                            }
                        }
                    }
                    else
                    {
                        if (!textRuleEvaluator.MatchesRule(jToken, rule))
                        {
                            failingRules.Add(rule);
                        }
                    }
                }
                catch (FormatException formatException)
                {
                    throw new FormatException($"{formatException.Message} in rule '{rule}'");
                }
            }
            if(failingRules.Any())
            {
                var aggregatedRules = failingRules.Aggregate((a,b) => a + "\n" + b);
                return ValidationResult.Invalid($"Does not match rule(s):\n{aggregatedRules}", Definition.Id, Definition.SubmitterEmail);
            }
            return ValidationResult.Valid();
        }

        private static bool IsConditional(string rule, out string validationRule, out string predicateRule)
        {
            validationRule = null;
            predicateRule = null;
            var match = Regex.Match(rule, "(?<ValidationRule>.*[^\\s])\\s+IF\\s+(?<PredicateRule>[^\\s].*)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;
            validationRule = match.Groups["ValidationRule"].Value;
            predicateRule = match.Groups["PredicateRule"].Value;
            return true;
        }

        public RulesetValidationResult IsRulesetSyntaxValid(string ruleset)
        {
            if (ruleset == null)
                return RulesetValidationResult.Invalid(ruleset);
            var rules = TextRulesetParser.ExtractRules(ruleset);
            var syntaxValidationResults = new List<RuleValidationResult>();
            foreach (var rule in rules)
            {
                RuleValidationResult ruleValidationResult;
                try
                {
                    if (IsConditional(rule, out var validationRule, out var predicateRule))
                    {
                        var validationPropertyName = new string(validationRule.TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
                        var predicatePropertyName = new string(predicateRule.TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
                        var jObject = TextRuleSyntaxCheckObjectGenerator.ConstructJObjectWithPropertyPath(validationPropertyName, predicatePropertyName);
                        if (textRuleEvaluator.MatchesRule(jObject, predicateRule))
                        {
                            textRuleEvaluator.MatchesRule(jObject, validationRule);
                        }
                    }
                    else
                    {
                        var propertyName = new string(rule.TakeWhile(c => !char.IsWhiteSpace(c)).ToArray());
                        var jObject = TextRuleSyntaxCheckObjectGenerator.ConstructJObjectWithPropertyPath(propertyName);
                        textRuleEvaluator.MatchesRule(jObject, rule);
                    }
                    ruleValidationResult = RuleValidationResult.Valid(rule);
                }
                catch (FormatException e)
                {
                    ruleValidationResult = RuleValidationResult.Invalid(rule, e.Message);
                }
                syntaxValidationResults.Add(ruleValidationResult);
            }
            return RulesetValidationResult.FromRuleValidationResults(ruleset, syntaxValidationResults);
        }
    }
}

