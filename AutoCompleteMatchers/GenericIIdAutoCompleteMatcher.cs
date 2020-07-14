using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCompleteMatchers.Helpers;
using DataAPI.Client.Repositories;
using Newtonsoft.Json.Linq;

namespace AutoCompleteMatchers
{
    public class GenericIIdAutoCompleteMatcher : IAutoCompleteMatcher<JObject>
    {
        public bool Match(JObject obj, string searchText)
        {
            if (obj == null)
                return false;
            if (!obj.ContainsKey("Id"))
                return false;
            var id = obj["Id"].Value<string>();
            return id.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        public double MatchQuality(JObject obj, string searchText)
        {
            return 0;
        }

        public Task<IEnumerable<JObject>> FindMatches(
            IReadonlyObjectDatabase<JObject> objectDatabase, 
            string searchText, 
            int maxSuggestions)
        {
            var normalizedSearchText = SearchTextNormalizer.Normalize(searchText);
            var query = $"Data.Id LIKE '%{normalizedSearchText}%'";
            return objectDatabase.SearchAsync(query, maxSuggestions);
        }
    }
}
