using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCompleteMatchers.Helpers;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace AutoCompleteMatchers
{
    public class DataCollectionProtocolAutoCompleteMatcher : IAutoCompleteMatcher<DataCollectionProtocol>
    {
        public bool Match(DataCollectionProtocol obj, string searchText)
        {
            return obj.Id.ToLowerInvariant().Contains(searchText.Trim().ToLowerInvariant());
        }

        public double MatchQuality(DataCollectionProtocol obj, string searchText)
        {
            if (obj.Id.StartsWith(searchText))
                return 100;
            if (obj.Id.Contains(searchText))
                return 90;
            if (obj.Id.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                return 80;
            return 0;
        }

        public async Task<IEnumerable<DataCollectionProtocol>> FindMatches(
            IReadonlyObjectDatabase<DataCollectionProtocol> objectDatabase, 
            string searchText, 
            int maxSuggestions)
        {
            var normalizedSearchText = SearchTextNormalizer.Normalize(searchText);
            var query = $"Data.Id LIKE '%{normalizedSearchText}%'";
            return await objectDatabase.SearchAsync(query, maxSuggestions);
        }
    }
}