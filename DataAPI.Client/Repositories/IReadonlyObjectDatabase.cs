using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAPI.Client.Repositories
{
    public interface IReadonlyObjectDatabase<T> : IOrderedQueryable<T>
    {
        void DeleteCache();
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> SearchAsync(string sqlWhereClause, int limit = -1);
        Task<T> GetFromIdAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}