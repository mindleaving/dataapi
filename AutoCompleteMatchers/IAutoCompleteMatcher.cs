using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;

namespace AutoCompleteMatchers
{
    public interface IAutoCompleteMatcher<T>
    {
        bool Match(T obj, string searchText);
        double MatchQuality(T obj, string searchText);
        Task<IEnumerable<T>> FindMatches(IReadonlyObjectDatabase<T> objectDatabase, string searchText, int maxSuggestions);
    }
}
