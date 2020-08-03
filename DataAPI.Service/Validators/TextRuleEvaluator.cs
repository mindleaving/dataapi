using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.Service.DataRouting;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public class TextRuleEvaluator
    {
        private List<TextRuleCommand> Commands { get; }

        public TextRuleEvaluator(IDataRouter dataRouter)
        {
            var referenceRuleEvaluator = new IsReferenceRuleEvaluator(dataRouter);
            Commands = new List<TextRuleCommand>
            {
                new TextRuleCommand("(NOT )?EXISTS", CheckExistance, false),

                new TextRuleCommand("IS( NOT)? LIST", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Array), true),
                new TextRuleCommand("IS( NOT)? ARRAY", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Array), true),
                new TextRuleCommand("IS( NOT)? STRING", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.String, JTokenType.Guid, JTokenType.Uri), false),
                new TextRuleCommand("IS( NOT)? TEXT", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.String, JTokenType.Guid, JTokenType.Uri), false),
                new TextRuleCommand("IS( NOT)? NUMBER", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Integer, JTokenType.Float), false),
                new TextRuleCommand("IS( NOT)? INT", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Integer), false),
                new TextRuleCommand("IS( NOT)? FLOAT", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Float), false),
                new TextRuleCommand("IS( NOT)? DATE(TIME)?", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Date), false),
                new TextRuleCommand("IS( NOT)? NULL", (jToken, regexMatch) => IsRuleEvaluator.CheckType(jToken, regexMatch, JTokenType.Null), false),
                new TextRuleCommand("IS( NOT)? EMPTY", IsRuleEvaluator.CheckEmpty, true),
                new TextRuleCommand("IS( NOT)?( EQUAL TO| EXACTLY)? ('.*'|\".*\"|[\\-0-9.]+)", IsRuleEvaluator.CheckEquality, false),
                new TextRuleCommand("IS( NOT)? BETWEEN ([\\-0-9.]+) AND ([\\-0-9.]+)( OR( IS)? NaN)?", (jToken, regexMatch) => IsRuleEvaluator.CheckComparison(jToken, regexMatch, ComparisonType.Between), false),
                new TextRuleCommand("IS( NOT)? (GREATER|MORE) THAN ([\\-0-9.]+)( OR( IS)? NaN)?", (jToken, regexMatch) => IsRuleEvaluator.CheckComparison(jToken, regexMatch, ComparisonType.GreaterThan), false),
                new TextRuleCommand("IS( NOT)? (GREATER|MORE) THAN OR (EQUAL TO|EXACTLY) ([\\-0-9.]+)( OR( IS)? NaN)?", (jToken, regexMatch) => IsRuleEvaluator.CheckComparison(jToken, regexMatch, ComparisonType.GreaterThanOrEqual), false),
                new TextRuleCommand("IS( NOT)? (LESS|SMALLER) THAN ([\\-0-9.]+)( OR( IS)? NaN)?", (jToken, regexMatch) => IsRuleEvaluator.CheckComparison(jToken, regexMatch, ComparisonType.LessThan), false),
                new TextRuleCommand("IS( NOT)? (LESS|SMALLER) THAN OR (EQUAL TO|EXACTLY) ([\\-0-9.]+)( OR( IS)? NaN)?", (jToken, regexMatch) => IsRuleEvaluator.CheckComparison(jToken, regexMatch, ComparisonType.LessThanOrEqual), false),

                new TextRuleCommand($"IS REFERENCE TO ({NamingConventions.CollectionNamePattern})", (jToken, regexMatch) => referenceRuleEvaluator.CheckValidReferenceAsync(jToken, regexMatch).Result, false),

                new TextRuleCommand("HAS( NOT)? (EXACTLY )?([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.Equal), true),
                new TextRuleCommand("HAS( NOT)? MORE THAN ([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.GreaterThan), true),
                new TextRuleCommand("HAS( NOT)? MORE THAN OR (EQUAL TO|EXACTLY) ([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.GreaterThanOrEqual), true),
                new TextRuleCommand("HAS( NOT)? LESS THAN ([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.LessThan), true),
                new TextRuleCommand("HAS( NOT)? LESS THAN OR (EQUAL TO|EXACTLY) ([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.LessThanOrEqual), true),
                new TextRuleCommand("HAS( NOT)? BETWEEN ([0-9]+) AND ([0-9]+) ITEMS", (jToken, regexMatch) => HasRuleEvaluator.CheckItemCount(jToken, regexMatch, ComparisonType.Between), true),

                new TextRuleCommand("HAS( NOT)? LENGTH( EQUAL TO| EXACTLY) ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.Equal), false),
                new TextRuleCommand("HAS( NOT)? LENGTH (GREATER|MORE) THAN ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.GreaterThan), false),
                new TextRuleCommand("HAS( NOT)? LENGTH (GREATER|MORE) THAN OR (EQUAL TO|EXACTLY) ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.GreaterThanOrEqual), false),
                new TextRuleCommand("HAS( NOT)? LENGTH (LESS|SMALLER) THAN ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.LessThan), false),
                new TextRuleCommand("HAS( NOT)? LENGTH (LESS|SMALLER) THAN OR (EQUAL TO|EXACTLY) ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.LessThanOrEqual), false),
                new TextRuleCommand("HAS( NOT)? LENGTH BETWEEN ([0-9]+) AND ([0-9]+)", (jToken, regexMatch) => HasLengthRuleEvaluator.CheckStringLength(jToken, regexMatch, ComparisonType.Between), false),

                new TextRuleCommand("(NOT )?MATCHES ([^\\s].*[^\\s])", (jToken, regexMatch) => MatchRuleEvaluator.CheckRegexMatch(jToken, regexMatch), false),
            };
        }

        public bool MatchesRule(JToken jToken, string rule)
        {
            var preprocessedRule = Preprocess(rule);
            var spaceSplittedRule = preprocessedRule.Split(new[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);
            if (spaceSplittedRule.Length < 2)
                throw new FormatException("Not enough information");
            var propertyPath = spaceSplittedRule[0];
            var command = spaceSplittedRule[1];
            var matchingCommand = Commands.FirstOrDefault(x => Regex.IsMatch(command, x.Pattern, RegexOptions.IgnoreCase));
            if (matchingCommand == null)
                throw new FormatException($"Unknown command '{command}'");

            var regexMatch = Regex.Match(command, matchingCommand.Pattern);
            foreach (var unwoundJToken in JsonUnwinder.Unwind(jToken, propertyPath))
            {
                if (unwoundJToken == null)
                {
                    var parentPath = RemoveLastPathLevel(propertyPath);
                    if (!string.IsNullOrEmpty(parentPath))
                    {
                        var parentToken = JsonUnwinder.Unwind(jToken, parentPath).FirstOrDefault();
                        if(parentToken == null || parentToken.Type == JTokenType.Null)
                            continue; // If the parent doesn't exist, we don't care about subrules. If parent is required, the corresponding EXISTS rule will fail anyway.
                    }
                }
                if (!matchingCommand.Predicate(unwoundJToken, regexMatch))
                    return false;
            }
            return true;
        }

        private static string RemoveLastPathLevel(string propertyPath)
        {
            var dotSplittedPropertyPath = propertyPath.Split('.');
            if (dotSplittedPropertyPath.Length == 1)
            {
                propertyPath = string.Empty;
            }
            else
            {
                propertyPath = dotSplittedPropertyPath
                    .Take(dotSplittedPropertyPath.Length - 1)
                    .Aggregate((a, b) => $"{a}.{b}");
            }

            return propertyPath;
        }

        private static string Preprocess(string rule)
        {
            return Regex.Replace(rule, "\\s+", " ").Trim();
        }

        private static bool CheckExistance(JToken jToken, Match regexMatch)
        {
            var negate = regexMatch.Groups[1].Success;
            var propertyExists = jToken != null;
            return negate ? !propertyExists : propertyExists;
        }

        private static bool TryGetProperty(JToken jToken, string propertyName, out JToken property)
        {
            property = jToken.SelectToken(propertyName);
            return property != null;
        }
    }

    public class TextRuleCommand
    {
        public TextRuleCommand(string pattern, Func<JToken, Match, bool> predicate, bool isArrayRule)
        {
            Pattern = pattern;
            Predicate = predicate;
            IsArrayRule = isArrayRule;
        }

        public string Pattern { get; }
        public Func<JToken, Match, bool> Predicate { get; }
        public bool IsArrayRule { get; }
    }

    public class AsyncTextRuleCommand
    {
        public AsyncTextRuleCommand(string pattern, Func<JToken, Match, Task<bool>> predicate, bool isArrayRule)
        {
            Pattern = pattern;
            Predicate = predicate;
            IsArrayRule = isArrayRule;
        }

        public string Pattern { get; }
        public Func<JToken, Match, Task<bool>> Predicate { get; }
        public bool IsArrayRule { get; }
    }
}
