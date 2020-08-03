using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;

namespace AutoCompleteMatchers
{
    public class StringAutoCompleteMatcher : IAutoCompleteMatcher<string>
    {
        public bool Match(string obj, string searchText)
        {
            return obj.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        public double MatchQuality(string obj, string searchText)
        {
            if (obj.StartsWith(searchText))
                return 100;
            if (obj.Contains(searchText))
                return 80;
            var objLower = obj.ToLowerInvariant();
            var searchTextLower = searchText.ToLowerInvariant();
            if (objLower.StartsWith(searchTextLower))
                return 60;
            if (objLower.Contains(searchTextLower))
                return 40;
            return 0;
        }

        public async Task<IEnumerable<string>> FindMatches(
            IReadonlyObjectDatabase<string> objectDatabase, 
            string searchText, 
            uint maxSuggestions)
        {
            return (await objectDatabase.GetManyAsync())
                .Where(str => Match(str, searchText))
                .Take((int)maxSuggestions);
        }
    }
}
