using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.UserManagement;

namespace SharedViewModels.Repositories
{
    public class UserDatabase : IReadonlyObjectDatabase<UserProfile>
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly Dictionary<string, UserProfile> cachedUserProfiles = new Dictionary<string, UserProfile>();
        private bool allItemsLoaded;

        public UserDatabase(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            Expression = Expression.Constant(this);
        }

        public void DeleteCache()
        {
            cachedUserProfiles.Clear();
            allItemsLoaded = false;
        }

        public async Task<IEnumerable<UserProfile>> GetManyAsync(
            Expression<Func<UserProfile, bool>> filter,
            string orderByClause = null,
            uint? limit = null)
        {
            if(orderByClause != null)
                throw new NotSupportedException("Ordering is not supported");
            if (!allItemsLoaded)
                await GetAllAsync();
            var items = cachedUserProfiles.Values.AsQueryable().Where(filter);
            if (limit.HasValue)
                items = items.Take((int)limit.Value);
            return items;
        }

        public async Task<IEnumerable<UserProfile>> GetManyAsync(
            string sqlWhereClause = null,
            string orderByClause = null,
            uint? limit = null)
        {
            if(orderByClause != null)
                throw new NotSupportedException("Ordering is not supported");
            if (!string.IsNullOrEmpty(sqlWhereClause))
                throw new NotSupportedException("Filtering using strings is not supported. Use overload with expression-filter instead");

            var items = await GetAllAsync();
            if (limit.HasValue)
                items = items.Take((int) limit.Value);
            return items;
        }

        public async Task<IEnumerable<UserProfile>> GetAllAsync()
        {
            if (allItemsLoaded)
                return cachedUserProfiles.Values;
            var items = await dataApiClient.GetAllUserProfiles();
            items
                .Where(item => !cachedUserProfiles.ContainsKey(item.Username))
                .ForEach(item => cachedUserProfiles.Add(item.Username, item));
            allItemsLoaded = true;
            return items;
        }

        public async Task<UserProfile> GetFromIdAsync(string id)
        {
            if (!allItemsLoaded)
                await GetAllAsync();
            return cachedUserProfiles.ContainsKey(id) ? cachedUserProfiles[id] : null;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await GetFromIdAsync(id) != null;
        }

        public IEnumerator<UserProfile> GetEnumerator()
        {
            return Task.Run(async () => await GetAllAsync()).Result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; }
        public Type ElementType { get; } = typeof(UserProfile);
        public IQueryProvider Provider { get; set; }
    }
}
