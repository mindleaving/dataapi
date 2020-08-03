using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public static class IsRuleEvaluator
    {
        public static bool CheckType(JToken jToken, Match regexMatch, params JTokenType[] jTokenTypes)
        {
            if (jToken == null)
                return false;
            var negate = regexMatch.Groups[1].Success;
            var typeMatches = jToken.Type.InSet(jTokenTypes);
            return negate ? !typeMatches : typeMatches;
        }

        public static bool CheckEmpty(JToken jToken, Match regexMatch)
        {
            if (jToken == null)
                return false;
            var negated = regexMatch.Groups[1].Success;
            bool predicate;
            switch (jToken.Type)
            {
                case JTokenType.Array:
                case JTokenType.Property:
                case JTokenType.Object:
                    predicate = !jToken.HasValues;
                    break;
                case JTokenType.Null:
                    predicate = true;
                    break;
                case JTokenType.String:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.Comment:
                    predicate = string.IsNullOrEmpty(jToken.Value<string>());
                    break;
                default:
                    predicate = !jToken.HasValues;
                    break;
            }
            return negated ? !predicate : predicate;
        }

        public static bool CheckEquality(JToken jToken, Match regexMatch)
        {
            if (jToken == null)
                return false;
            var negated = regexMatch.Groups[1].Success;
            var comparisonValue = regexMatch.Groups[3].Value;
            var quotes = new[] {'\'', '\"'};
            if (comparisonValue.First().InSet(quotes) && comparisonValue.Last().InSet(quotes))
            {
                if (!jToken.Type.InSet(JTokenType.String, JTokenType.Uri, JTokenType.Guid))
                    return false;
                comparisonValue = comparisonValue.Substring(1, comparisonValue.Length - 2);
                var predicate = jToken.Value<string>() == comparisonValue;
                return negated ? !predicate : predicate;
            }
            else
            {
                var predicate = jToken.Value<string>() == comparisonValue;
                return negated ? !predicate : predicate;
            }
        }

        public static bool CheckComparison(JToken jToken, Match regexMatch, ComparisonType comparisonType)
        {
            if (jToken == null)
                return false;
            var negated = regexMatch.Groups[1].Success;
            double value;
            if(jToken.Value<string>().ToLowerInvariant() == "nan")
                value = double.NaN;
            else if (!double.TryParse(jToken.Value<string>(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return false;
            double comparisonValue;
            bool predicate;
            bool allowNaN;
            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    comparisonValue = double.Parse(regexMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                    allowNaN = false;
                    if (value == comparisonValue)
                        predicate = true;
                    else if (value.Abs() > 1e-6)
                        predicate = Math.Abs((value - comparisonValue) / value) < 1e-3;
                    else
                        predicate = Math.Abs(value - comparisonValue) < 1e-6;
                    break;
                case ComparisonType.GreaterThan:
                    comparisonValue = double.Parse(regexMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                    allowNaN = regexMatch.Groups[4].Success;
                    predicate = value > comparisonValue;
                    break;
                case ComparisonType.GreaterThanOrEqual:
                    comparisonValue = double.Parse(regexMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                    allowNaN = regexMatch.Groups[5].Success;
                    predicate = value >= comparisonValue;
                    break;
                case ComparisonType.LessThan:
                    comparisonValue = double.Parse(regexMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                    allowNaN = regexMatch.Groups[4].Success;
                    predicate = value < comparisonValue;
                    break;
                case ComparisonType.LessThanOrEqual:
                    comparisonValue = double.Parse(regexMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                    allowNaN = regexMatch.Groups[5].Success;
                    predicate = value <= comparisonValue;
                    break;
                case ComparisonType.Between:
                    var limit1 = double.Parse(regexMatch.Groups[2].Value, CultureInfo.InvariantCulture);
                    var limit2 = double.Parse(regexMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                    allowNaN = regexMatch.Groups[4].Success;
                    var lowerLimit = Math.Min(limit1, limit2);
                    var upperLimit = Math.Max(limit1, limit2);
                    predicate = value >= lowerLimit && value <= upperLimit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null);
            }
            if (value.IsNaN() && allowNaN)
                return true;
            return negated ? !predicate : predicate;
        }
    }
}
