using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataAPI.Service.Search
{
    public static class QueryParameterInserter
    {
        private const string ForbiddenCharacters = "{}[]'\"";

        public static string InsertParameters(string query, Dictionary<string, List<string>> queryCollection)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (queryCollection == null) throw new ArgumentNullException(nameof(queryCollection));

            var parameterGroup = Regex.Matches(query, @"{([a-zA-Z0-9_]+)}");
            var filledInQuery = query;
            foreach (Match match in parameterGroup)
            {
                var parameterName = match.Groups[1].Value;
                if(!queryCollection.ContainsKey(parameterName))
                    throw new FormatException($"Parameter '{parameterName}' is not provided with request");
                var values = queryCollection[parameterName].ToList();
                if(values.Count != 1)
                    throw new FormatException($"Parameter '{parameterName}' has multiple values");
                var replacement = values.Single();
                if(replacement.Intersect(ForbiddenCharacters).Any())
                    throw new FormatException($"Parameter '{parameterName}' contains illegal characters");
                filledInQuery = filledInQuery.Replace(match.Value, replacement);
            }
            return filledInQuery;
        }
    }
}
