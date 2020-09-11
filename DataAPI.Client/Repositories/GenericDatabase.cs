using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Exceptions;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Repositories
{
    public class GenericDatabase : IObjectDatabase<JObject>
    {
        private readonly IDataApiClient dataApiClient;
        private readonly string parsedPermanentFilter;
        private readonly ConcurrentDictionary<string, JObject> cachedItems = new ConcurrentDictionary<string, JObject>();

        public GenericDatabase(IDataApiClient dataApiClient, string collectionName, string permanentFilter = null)
        {
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));
            parsedPermanentFilter = permanentFilter;
            ElementType = typeof(JObject);
            Provider = new DataApiQueryProvider<JObject>(dataApiClient);
            Expression = Expression.Constant(this);
            CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
        }

        public string CollectionName { get; }

        public void DeleteCache()
        {
            cachedItems.Clear();
        }

        public async Task<IEnumerable<JObject>> GetManyAsync(string sqlWhereClause, string orderByClause = null, uint? limit = null)
        {
            EnsureLoggedIn();
            var combinedWhereClause = WhereClauseCombiner.CombinedWhereClause(parsedPermanentFilter, sqlWhereClause);
            var items = (await dataApiClient.GetManyAsync(CollectionName, combinedWhereClause, orderByClause, limit))
                .Select(JObject.Parse)
                .ToList();
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            return items;
        }

        public Task<IEnumerable<JObject>> GetManyAsync(Expression<Func<JObject, bool>> filter, string orderByClause = null, uint? limit = null)
        {
            throw new NotSupportedException("Use a typed GenericDatabase if you want to use expressions for searching");
        }

        public async Task<JObject> GetFromIdAsync(string id)
        {
            EnsureLoggedIn();
            if (cachedItems.TryGetValue(id, out var item))
                return item;
            try
            {
                item = JObject.Parse(await dataApiClient.GetAsync(CollectionName, id));
                if(item != null)
                    cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item);
                return item;
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == HttpStatusCode.NotFound)
                    return default;
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            EnsureLoggedIn();
            return await dataApiClient.ExistsAsync(CollectionName, id);
        }

        public async Task StoreAsync(JObject item)
        {
            EnsureLoggedIn();
            var id = GetId(item);
            await dataApiClient.ReplaceAsync(item, id);
            cachedItems.AddOrUpdate(id, item, (s, existingItem) => item);
        }

        public async Task DeleteAsync(string id)
        {
            EnsureLoggedIn();
            await dataApiClient.DeleteAsync(CollectionName, id);
            if(cachedItems.ContainsKey(id))
                cachedItems.TryRemove(id, out _);
        }

        private void EnsureLoggedIn()
        {
            if (!dataApiClient.IsLoggedIn)
                throw new Exception("You are not logged in!");
        }

        public IEnumerator<JObject> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<JObject>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType { get; }
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        private string GetId(JObject item)
        {
            return item["Id"].Value<string>();
        }
    }

    public class GenericDatabase<T> : IObjectDatabase<T> where T: IId
    {
        private readonly IDataApiClient dataApiClient;
        private readonly string parsedPermanentFilter;
        private readonly ConcurrentDictionary<string, T> cachedItems = new ConcurrentDictionary<string, T>();

        public GenericDatabase(IDataApiClient dataApiClient, Expression<Func<T, bool>> permanentFilter)
        {
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));
            if (permanentFilter != null)
                parsedPermanentFilter = ExpressionParser.ParseWhereExpression(permanentFilter);
            ElementType = typeof(T);
            Provider = new DataApiQueryProvider<T>(dataApiClient);
            Expression = Expression.Constant(this);
        }

        public GenericDatabase(IDataApiClient dataApiClient, string permanentFilter = null)
        {
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));
            parsedPermanentFilter = permanentFilter;
            ElementType = typeof(T);
            Provider = new DataApiQueryProvider<T>(dataApiClient);
            Expression = Expression.Constant(this);
        }

        public void DeleteCache()
        {
            cachedItems.Clear();
        }

        public async Task<IEnumerable<T>> GetManyAsync(string sqlWhereClause = null, string orderByClause = null, uint? limit = null)
        {
            EnsureLoggedIn();
            var combinedWhereClause = WhereClauseCombiner.CombinedWhereClause(parsedPermanentFilter, sqlWhereClause);
            var items = await dataApiClient.GetManyAsync<T>(combinedWhereClause, orderByClause, limit);
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            return items;
        }

        public async Task<IEnumerable<T>> GetManyAsync(Expression<Func<T,bool>> filter, string orderByClause = null, uint? limit = null)
        {
            var sqlWhereClause = ExpressionParser.ParseWhereExpression(filter);
            return await GetManyAsync(sqlWhereClause, orderByClause, limit);
        }

        public async Task<T> GetFromIdAsync(string id)
        {
            EnsureLoggedIn();
            if (cachedItems.TryGetValue(id, out var item))
                return item;
            try
            {
                item = await dataApiClient.GetAsync<T>(id);
                if(item != null)
                    cachedItems.AddOrUpdate(id, item, (key, existingItem) => item);
                return item;
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == HttpStatusCode.NotFound)
                    return default;
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            EnsureLoggedIn();
            return await dataApiClient.ExistsAsync<T>(id);
        }

        public async Task StoreAsync(T item)
        {
            EnsureLoggedIn();
            var id = GetId(item);
            await dataApiClient.ReplaceAsync(item, id);
            cachedItems.AddOrUpdate(id, item, (key, existingItem) => item);
        }

        public async Task DeleteAsync(string id)
        {
            EnsureLoggedIn();
            await dataApiClient.DeleteAsync<T>(id);
            if(cachedItems.ContainsKey(id))
                cachedItems.TryRemove(id, out _);
        }

        private void EnsureLoggedIn()
        {
            if (!dataApiClient.IsLoggedIn)
                throw new Exception("You are not logged in!");
        }

        private static string GetId(T item)
        {
            return item.Id;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType { get; }
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
    }
}