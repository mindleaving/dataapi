using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAPI.Client.Repositories;

namespace SharedViewModels.Repositories
{
    public class FixedSetDatabase<T> : IReadonlyObjectDatabase<T>
    {
        private readonly IDictionary<string, T> items;

        public FixedSetDatabase(IDictionary<string, T> items)
        {
            this.items = items;
            Expression = Expression.Constant(this);
            ElementType = typeof(T);
        }

        public void DeleteCache()
        {
            // Do nothing
        }

        public Task<IEnumerable<T>> GetManyAsync(
            Expression<Func<T, bool>> filter,
            string orderByClause = null,
            uint? limit = null)
        {
            return Task.FromResult<IEnumerable<T>>(items.Values.AsQueryable().Where(filter));
        }

        public Task<IEnumerable<T>> GetManyAsync(string sqlWhereClause = null, string orderByClause = null, uint? limit = null)
        {
            if(string.IsNullOrEmpty(sqlWhereClause))
                return Task.FromResult<IEnumerable<T>>(items.Values);
            throw new NotSupportedException();
        }

        public Task<T> GetFromIdAsync(string id)
        {
            return Task.FromResult(items.ContainsKey(id) ? items[id] : default);
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(items.ContainsKey(id));
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; }
        public Type ElementType { get; }
        public IQueryProvider Provider { get; }
    }
}
