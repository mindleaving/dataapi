using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DomainModels;

namespace AutoCompleteMatchers
{
    public class DataServiceDefinitionAutoCompleteMatcher<TDataServiceDefinition,TDataServiceDefinitionField> : IAutoCompleteMatcher<TDataServiceDefinition>
        where TDataServiceDefinition: IDataServiceDefinition<TDataServiceDefinitionField>
        where TDataServiceDefinitionField: IDataServiceDefinitionField
    {
        public bool Match(TDataServiceDefinition obj, string searchText)
        {
            var lowerSearchText = searchText.ToLowerInvariant();
            if (obj.DataType.ToLowerInvariant().Contains(lowerSearchText))
                return true;
            if (obj.OwnerInitials.ToLowerInvariant().Contains(lowerSearchText))
                return true;
            return false;
        }

        public double MatchQuality(TDataServiceDefinition obj, string searchText)
        {
            var lowerSearchText = searchText.ToLowerInvariant();
            if (obj.OwnerInitials.ToLowerInvariant().StartsWith(lowerSearchText))
                return 100;
            if (obj.DataType.StartsWith(searchText))
                return 100;
            if (obj.DataType.ToLowerInvariant().StartsWith(lowerSearchText))
                return 80;
            if (obj.OwnerInitials.ToLowerInvariant().Contains(lowerSearchText))
                return 80;
            if (obj.DataType.ToLowerInvariant().Contains(lowerSearchText))
                return 60;
            return 0;
        }

        public async Task<IEnumerable<TDataServiceDefinition>> FindMatches(
            IReadonlyObjectDatabase<TDataServiceDefinition> objectDatabase,
            string searchText,
            int maxSuggestions)
        {
            var query = $"Data.Id LIKE '%{searchText}%'";
            return await objectDatabase.SearchAsync(query, maxSuggestions);
        }
    }
}
