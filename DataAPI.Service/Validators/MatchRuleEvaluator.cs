using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public static class MatchRuleEvaluator
    {
        public static bool CheckRegexMatch(JToken jToken, Match regexMatch)
        {
            if (jToken == null)
                return false;
            var negate = regexMatch.Groups[1].Success;

            if (jToken.Type != JTokenType.String)
                return false;
            var pattern = regexMatch.Groups[2].Value;
            var predicate = Regex.IsMatch(jToken.Value<string>(), pattern);

            return negate ? !predicate : predicate;
        }
    }
}
