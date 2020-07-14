using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using DataAPI.DataStructures.Exceptions;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Repositories
{
    public class GenericDatabase : IObjectDatabase<JObject>
    {
        private readonly IDataApiClient dataApiClient;
        private readonly ConcurrentDictionary<string, JObject> cachedItems = new ConcurrentDictionary<string, JObject>();
        private bool allItemsLoaded;

        public GenericDatabase(IDataApiClient dataApiClient, string collectionName)
        {
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));
            ElementType = typeof(JObject);
            Provider = new DataApiQueryProvider<JObject>(dataApiClient);
            Expression = Expression.Constant(this);
            CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
        }

        public string CollectionName { get; }

        public void DeleteCache()
        {
            cachedItems.Clear();
            allItemsLoaded = false;
        }

        public async Task<IEnumerable<JObject>> GetAllAsync()
        {
            EnsureLoggedIn();
            if (allItemsLoaded)
                return cachedItems.Values;
            var items = (await dataApiClient.GetManyAsync(CollectionName))
                .Select(JObject.Parse)
                .ToList();
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            allItemsLoaded = true;
            return items;
        }

        public async Task<IEnumerable<JObject>> SearchAsync(string sqlWhereClause, int limit)
        {
            EnsureLoggedIn();
            var items = (await dataApiClient.GetManyAsync(CollectionName, sqlWhereClause))
                .Select(JObject.Parse)
                .ToList();
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            return items;
        }

        public async Task<JObject> GetFromIdAsync(string id)
        {
            EnsureLoggedIn();
            if (allItemsLoaded && !cachedItems.ContainsKey(id))
                return default;
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

    public class GenericDatabase<T> : IObjectDatabase<T>
    {
        private readonly IDataApiClient dataApiClient;
        private readonly ConcurrentDictionary<string, T> cachedItems = new ConcurrentDictionary<string, T>();
        private bool allItemsLoaded;

        public GenericDatabase(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient ?? throw new ArgumentNullException(nameof(dataApiClient));
            ElementType = typeof(T);
            Provider = new DataApiQueryProvider<T>(dataApiClient);
            Expression = Expression.Constant(this);
        }

        public void DeleteCache()
        {
            cachedItems.Clear();
            allItemsLoaded = false;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            EnsureLoggedIn();
            if (allItemsLoaded)
                return cachedItems.Values;
            var items = await dataApiClient.GetManyAsync<T>();
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            allItemsLoaded = true;
            return items;
        }

        public async Task<IEnumerable<T>> SearchAsync(string sqlWhereClause, int limit)
        {
            EnsureLoggedIn();
            var items = await dataApiClient.GetManyAsync<T>(sqlWhereClause);
            items.ForEach(item => cachedItems.AddOrUpdate(GetId(item), item, (key, existingItem) => item));
            return items;
        }

        public async Task<T> GetFromIdAsync(string id)
        {
            EnsureLoggedIn();
            if (allItemsLoaded && !cachedItems.ContainsKey(id))
                return default;
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
            dynamic d = item;
            return d.Id.ToString();
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