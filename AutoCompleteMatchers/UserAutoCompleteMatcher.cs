using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;

namespace AutoCompleteMatchers
{
    public class UserAutoCompleteMatcher : IAutoCompleteMatcher<UserProfile>
    {
        public bool Match(UserProfile obj, string searchText)
        {
            var lowerSearchText = searchText.ToLowerInvariant().Trim();
            var searchTextParts = lowerSearchText.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var searchTextPart in searchTextParts)
            {
                var partMatches = (obj.FirstName?.ToLowerInvariant().Contains(searchTextPart) ?? false)
                                  || (obj.LastName?.ToLowerInvariant().Contains(searchTextPart) ?? false)
                                  || obj.Username.ToLowerInvariant().Contains(searchTextPart);
                if (!partMatches)
                    return false;
            }
            return true;
        }

        public double MatchQuality(UserProfile obj, string searchText)
        {
            var qualitySum = 0;
            var lowerSearchText = searchText.ToLowerInvariant().Trim();
            var searchTextParts = lowerSearchText.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var searchTextPart in searchTextParts)
            {
                var matchesFirstName = obj.FirstName?.ToLowerInvariant().Contains(searchTextPart) ?? false;
                if (matchesFirstName)
                {
                    qualitySum += 10;
                    continue;
                }
                var matchesLastName = obj.LastName?.ToLowerInvariant().Contains(searchTextPart) ?? false;
                if (matchesLastName)
                {
                    qualitySum += 10;
                    continue;
                }
                var matchesUsername = obj.Username.ToLowerInvariant().Contains(searchTextPart);
                if (matchesUsername)
                {
                    qualitySum += 10;
                }
            }

            return qualitySum;
        }

        public async Task<IEnumerable<UserProfile>> FindMatches(
            IReadonlyObjectDatabase<UserProfile> objectDatabase, 
            string searchText, 
            uint maxSuggestions)
        {
            var allUserProfiles = await objectDatabase.GetManyAsync(); // User profile SQL queries is not supported
                                                                      // and hence we have to do the search ourself
            return allUserProfiles.Where(user => Match(user, searchText));
        }
    }
}
