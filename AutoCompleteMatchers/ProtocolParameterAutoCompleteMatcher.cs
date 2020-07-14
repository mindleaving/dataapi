using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCompleteMatchers.Helpers;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace AutoCompleteMatchers
{
    public class ProtocolParameterAutoCompleteMatcher : IAutoCompleteMatcher<DataCollectionProtocolParameter>
    {
        public bool Match(DataCollectionProtocolParameter obj, string searchText)
        {
            if (obj.Name.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                return true;
            return false;
        }

        public double MatchQuality(DataCollectionProtocolParameter obj, string searchText)
        {
            if (obj.Name.StartsWith(searchText))
                return 100;
            if (obj.Name.Contains(searchText))
                return 80;
            var objLower = obj.Name.ToLowerInvariant();
            var searchTextLower = searchText.ToLowerInvariant();
            if (objLower.StartsWith(searchTextLower))
                return 60;
            if (objLower.Contains(searchTextLower))
                return 40;
            return 0;
        }

        public async Task<IEnumerable<DataCollectionProtocolParameter>> FindMatches(
            IReadonlyObjectDatabase<DataCollectionProtocolParameter> objectDatabase,
            string searchText,
            int maxSuggestions)
        {
            var normalizedSearchText = SearchTextNormalizer.Normalize(searchText);
            var query = $"Data.Name LIKE '%{normalizedSearchText}%'";
            return await objectDatabase.SearchAsync(query, maxSuggestions);
        }
    }
}
