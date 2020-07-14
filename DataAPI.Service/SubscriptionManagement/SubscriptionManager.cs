using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Search;
using MongoDB.Driver;

namespace DataAPI.Service.SubscriptionManagement
{
    public class SubscriptionManager
    {
        private readonly IDataRouter dataRouter;
        private readonly IMongoCollection<Subscription> subscriptionCollection;
        private readonly IMongoCollection<DataNotification> dataNotificationCollection;

        public SubscriptionManager(RdDataMongoClient rdDataMongoClient, IDataRouter dataRouter)
        {
            this.dataRouter = dataRouter;
            subscriptionCollection = rdDataMongoClient.BackendDatabase.GetCollection<Subscription>(nameof(Subscription));
            dataNotificationCollection = rdDataMongoClient.BackendDatabase.GetCollection<DataNotification>(nameof(DataNotification));
        }

        public async Task<bool> HasExistingSubscriptionAsync(SubscriptionBody subscriptionBody, string username)
        {
            using var cursor = await subscriptionCollection.Find(
                    x =>
                        x.Username == username
                        && x.DataType == subscriptionBody.DataType
                        && x.Filter == subscriptionBody.Filter)
                .ToCursorAsync();
            while (await cursor.MoveNextAsync())
            {
                foreach (var subscription in cursor.Current)
                {
                    var matchesNewSubscription = subscription.ModificationTypes.Distinct().ToList()
                        .Equivalent(subscriptionBody.ModificationTypes.Distinct().ToList());
                    if (matchesNewSubscription)
                        return true;
                }
            }
            return false;
        }

        public async Task<string> SubscribeAsync(SubscriptionBody subscriptionBody, string username)
        {
            var subscription = new Subscription(
                username,
                subscriptionBody.DataType,
                subscriptionBody.ModificationTypes.Distinct().ToList(),
                subscriptionBody.Filter);
            await subscriptionCollection.InsertOneAsync(subscription);
            return subscription.Id;
        }

        public Task UnsubscribeAsync(string username, string id)
        {
            return subscriptionCollection.DeleteOneAsync(x => x.Username == username && x.Id == id);
        }

        public Task UnsubscribeAllAsync(string username, string dataType)
        {
            return subscriptionCollection.DeleteManyAsync(x => x.Username == username && x.DataType == dataType);
        }

        public async Task NotifyDataChangedAsync(string dataType, string dataObjectId, DataModificationType modificationType)
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (dataObjectId == null)
                throw new ArgumentNullException(nameof(dataObjectId));

            using var cursor = await subscriptionCollection
                .Find(x => x.DataType == dataType || x.DataType == null)
                .ToCursorAsync();
            var subscribedUsers = new HashSet<string>();
            while (await cursor.MoveNextAsync())
            {
                foreach (var subscription in cursor.Current)
                {
                    if(subscribedUsers.Contains(subscription.Username))
                        continue;
                    if(!await MatchesFilterAsync(dataType, dataObjectId, subscription.Filter))
                        continue;
                    subscribedUsers.Add(subscription.Username);
                }
            }
            foreach (var subscribedUser in subscribedUsers)
            {
                var notification = new DataNotification(
                    subscribedUser,
                    dataType,
                    dataObjectId, 
                    modificationType);
                await dataNotificationCollection.InsertOneAsync(notification);
            }
        }

        public async Task NotifyUserAboutNewDataAsync(string username, string dataType, string dataObjectId)
        {
            var notification = new DataNotification(username, dataType, dataObjectId, DataModificationType.Created);
            await dataNotificationCollection.InsertOneAsync(notification);
        }

        private async Task<bool> MatchesFilterAsync(string dataType, string dataObjectId, string filter)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(dataType));
            if (string.IsNullOrEmpty(filter))
                return true;
            var query = $"SELECT * FROM {dataType} WHERE _id = '{dataObjectId}' AND ({filter})";
            var parsedQuery = DataApiSqlQueryParser.Parse(query);
            var collection = await dataRouter.GetSourceSystemAsync(dataType);
            return await collection.SearchAsync(parsedQuery).AnyAsync();
        }

        public IAsyncEnumerable<SubscriptionNotification> GetSubscribedObjectsAsync(string username, string dataType)
        {
            var notifications = string.IsNullOrEmpty(dataType)
                ? dataNotificationCollection.Find(x => x.Username == username)
                : dataNotificationCollection.Find(x => x.Username == username && x.DataType == dataType);
            return notifications.ToAsyncEnumerable().Select(x => new SubscriptionNotification(x.Id, x.DataType, x.DataObjectId, x.ModificationType));
        }

        public Task<DataNotification> GetNotificationByIdAsync(string notificationId)
        {
            return dataNotificationCollection.Find(x => x.Id == notificationId).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            var deletionResult = await dataNotificationCollection.DeleteOneAsync(x => x.Id == notificationId);
            return deletionResult.IsAcknowledged && deletionResult.DeletedCount == 1;
        }

        public IAsyncEnumerable<Subscription> GetSubscriptionsAsync(string dataType, string username)
        {
            if (string.IsNullOrEmpty(dataType))
                return subscriptionCollection.Find(x => x.Username == username).ToAsyncEnumerable();
            return subscriptionCollection.Find(x => x.Username == username && x.DataType == dataType).ToAsyncEnumerable();
        }

        public Task<Subscription> GetSubscriptionByIdAsync(string id)
        {
            return subscriptionCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
    }
}
