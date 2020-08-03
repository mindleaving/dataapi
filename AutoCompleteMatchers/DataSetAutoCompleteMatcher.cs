using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCompleteMatchers.Helpers;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace AutoCompleteMatchers
{
    public class DataSetAutoCompleteMatcher : IAutoCompleteMatcher<DataSet>
    {
        public bool Match(DataSet obj, string searchText)
        {
            return obj.Id.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        public double MatchQuality(DataSet obj, string searchText)
        {
            if (obj.Id.StartsWith(searchText))
                return 100;
            if (obj.Id.Contains(searchText))
                return 80;
            if (obj.Id.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
                return 10;
            return 0;
        }

        public async Task<IEnumerable<DataSet>> FindMatches(
            IReadonlyObjectDatabase<DataSet> objectDatabase, 
            string searchText, 
            uint maxSuggestions)
        {
            var normalizedSearchText = SearchTextNormalizer.Normalize(searchText);
            var searchQuery = $"Data.{nameof(DataSet.Id)} LIKE '%{normalizedSearchText}%'";
            return await objectDatabase.GetManyAsync(searchQuery, limit: maxSuggestions);
        }
    }
}
