using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAPI.Client.Repositories
{
    public interface IReadonlyObjectDatabase<T> : IOrderedQueryable<T>
    {
        void DeleteCache();
        Task<IEnumerable<T>> GetManyAsync(string sqlWhereClause = null, string orderByClause = null, uint? limit = null);
        Task<IEnumerable<T>> GetManyAsync(Expression<Func<T, bool>> filter, string orderByClause = null, uint? limit = null);
        Task<T> GetFromIdAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}