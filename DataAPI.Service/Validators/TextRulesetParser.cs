using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAPI.Service.Validators
{
    public static class TextRulesetParser
    {
        public static IEnumerable<string> ExtractRules(string ruleset)
        {
            var singleLineRules = ruleset.RemoveLineBreaks(";");
            var rules = singleLineRules
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
            return rules;
        }
    }
}
