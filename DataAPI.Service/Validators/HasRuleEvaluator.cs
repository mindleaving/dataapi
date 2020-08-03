using System;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public static class HasRuleEvaluator
    {
        public static bool CheckItemCount(JToken jToken, Match regexMatch, ComparisonType comparisonType)
        {
            if (jToken == null)
                return false;
            var negated = regexMatch.Groups[1].Success;
            int limit;
            bool predicate;
            var itemCount = jToken.Children().Count();
            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    limit = int.Parse(regexMatch.Groups[3].Value);
                    predicate = itemCount == limit;
                    break;
                case ComparisonType.GreaterThan:
                    limit = int.Parse(regexMatch.Groups[2].Value);
                    predicate = itemCount > limit;
                    break;
                case ComparisonType.GreaterThanOrEqual:
                    limit = int.Parse(regexMatch.Groups[3].Value);
                    predicate = itemCount >= limit;
                    break;
                case ComparisonType.LessThan:
                    limit = int.Parse(regexMatch.Groups[2].Value);
                    predicate = itemCount < limit;
                    break;
                case ComparisonType.LessThanOrEqual:
                    limit = int.Parse(regexMatch.Groups[3].Value);
                    predicate = itemCount <= limit;
                    break;
                case ComparisonType.Between:
                    var limit1 = int.Parse(regexMatch.Groups[2].Value);
                    var limit2 = int.Parse(regexMatch.Groups[3].Value);
                    predicate = itemCount.IsBetween(limit1, limit2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null);
            }
            return negated ? !predicate : predicate;
        }
    }
}
