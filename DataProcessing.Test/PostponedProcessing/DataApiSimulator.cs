using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.UserManagement;
using Moq;
using Newtonsoft.Json;

namespace DataProcessing.Test.PostponedProcessing
{
    public class DataApiSimulator
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> database = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private readonly ConcurrentDictionary<string, List<SubscriptionInfo>> subscriptions = new ConcurrentDictionary<string, List<SubscriptionInfo>>();
        private readonly ConcurrentDictionary<string, SubscriptionNotification> subscriptionNotifications = new ConcurrentDictionary<string, SubscriptionNotification>();

        public DataApiSimulator()
        {
            DataApiClientMock = BuildDataApiClientMock();
        }

        private Mock<IDataApiClient> BuildDataApiClientMock()
        {
            var dataApiClientMock = new Mock<IDataApiClient>();
            dataApiClientMock.Setup(x => x.IsAvailable()).Returns(true);
            dataApiClientMock.Setup(x => x.IsLoggedIn).Returns(true);
            dataApiClientMock.Setup(x => x.LoggedInUsername).Returns("jdoe");
            var apiConfiguration = new ApiConfiguration("", 443);
            dataApiClientMock.Setup(x => x.ApiConfiguration).Returns(apiConfiguration);
            dataApiClientMock.Setup(x => x.LoginMethod).Returns(LoginMethod.ActiveDirectory);
            dataApiClientMock
                .Setup(x => x.InsertAsync(It.IsAny<object>(), It.IsAny<string>()))
                .Returns<object, string>((obj, id) => Task.FromResult(InsertData(obj, id)));
            dataApiClientMock
                .Setup(x => x.InsertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((dataType, json, id) => Task.FromResult(InsertData(dataType, json, id)));
            dataApiClientMock
                .Setup(x => x.ReplaceAsync(It.IsAny<object>(), It.IsAny<string>()))
                .Returns<object, string>((obj, id) => Task.FromResult(InsertData(obj, id)));
            dataApiClientMock
                .Setup(x => x.ReplaceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((dataType, json, id) => Task.FromResult(InsertData(dataType, json, id)));
            dataApiClientMock
                .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((dataType, id) => Task.FromResult(Exists(dataType, id)));
            dataApiClientMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((dataType, id) => Task.FromResult(Get(dataType, id)));
            dataApiClientMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((dataType, id) => Delete(dataType, id))
                .Returns(Task.CompletedTask);
            dataApiClientMock
                .Setup(x => x.SubscribeAsync(It.IsAny<string>(), It.IsAny<IList<DataModificationType>>(), It.IsAny<string>()))
                .Returns<string, IList<DataModificationType>, string>((dataType, modificationTypes, filter) => Task.FromResult(CreateSubscription(dataType, modificationTypes, filter)));
            dataApiClientMock
                .Setup(x => x.GetSubscribedObjects(It.IsAny<string>()))
                .Returns<string>(dataType => Task.FromResult(GetSubscriptionNotifications(dataType)));
            dataApiClientMock
                .Setup(x => x.DeleteNotificationAsync(It.IsAny<string>()))
                .Returns<string>(notificationId => Task.FromResult(subscriptionNotifications.TryRemove(notificationId, out _)));
            dataApiClientMock
                .Setup(x => x.GetSubscriptionsAsync(It.IsAny<string>()))
                .Returns<string>(dataType => Task.FromResult(GetSubscriptions(dataType)));
            return dataApiClientMock;
        }

        public Mock<IDataApiClient> DataApiClientMock { get; }
        public IDataApiClient DataApiClient => DataApiClientMock.Object;

        private string InsertData(
            object obj,
            string id = null)
        {
            return InsertData(obj.GetType().Name, JsonConvert.SerializeObject(obj), id);
        }

        private string InsertData(
            string dataType, 
            string json, 
            string id)
        {
            if (!database.ContainsKey(dataType))
                database.TryAdd(dataType, new ConcurrentDictionary<string, string>());
            var collection = database[dataType];
            if (id == null)
                id = IdGenerator.FromGuid();
            var modificationType = collection.ContainsKey(id) ? DataModificationType.Replaced : DataModificationType.Created;
            collection.AddOrUpdate(id, json, (key, oldValue) => json);
            var triggerSubscriptionNotification = subscriptions.Values
                .SelectMany(x => x)
                .Any(x => x.DataType == dataType && x.ModificationTypes.Contains(modificationType));
            if(triggerSubscriptionNotification)
            {
                var subscriptionNotification = new SubscriptionNotification(
                    IdGenerator.FromGuid(),
                    dataType,
                    id,
                    modificationType);
                subscriptionNotifications.TryAdd(subscriptionNotification.Id, subscriptionNotification);
            }
            return id;
        }

        private bool Exists(string dataType, string id)
        {
            if (!database.TryGetValue(dataType, out var collection))
                return false;
            return collection.ContainsKey(id);
        }

        public T Get<T>(string id)
        {
            var dataType = typeof(T).Name;
            if (!database.TryGetValue(dataType, out var collection))
                return default;
            if (!collection.TryGetValue(id, out var json))
                return default;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private string Get(string dataType, string id)
        {
            if (!database.TryGetValue(dataType, out var collection))
                return null;
            if (!collection.TryGetValue(id, out var json))
                return null;
            return json;
        }

        public void Delete<T>(string id)
        {
            var dataType = typeof(T).Name;
            Delete(dataType, id);
        }

        private void Delete(string dataType, string id)
        {
            if (!database.TryGetValue(dataType, out var collection))
                return;
            if (collection.TryRemove(id, out _))
            {
                var subscriptionNotification = new SubscriptionNotification(
                    IdGenerator.FromGuid(),
                    dataType,
                    id,
                    DataModificationType.Deleted);
                subscriptionNotifications.TryAdd(subscriptionNotification.Id, subscriptionNotification);
            }
        }

        private string CreateSubscription(
            string dataType,
            IEnumerable<DataModificationType> modificationTypes,
            string filter)
        {
            var subscriptionInfo = new SubscriptionInfo(
                IdGenerator.FromGuid(),
                dataType,
                modificationTypes.ToList(),
                filter);
            subscriptions.AddOrUpdate(
                dataType,
                new List<SubscriptionInfo> {subscriptionInfo},
                (key, list) =>
                {
                    list.Add(subscriptionInfo);
                    return list;
                });
            return subscriptionInfo.Id;
        }

        private List<SubscriptionNotification> GetSubscriptionNotifications(string dataType)
        {
            if(dataType == null)
                return subscriptionNotifications.Values.ToList();
            return subscriptionNotifications.Values.Where(x => x.DataType == dataType).ToList();
        }

        private List<SubscriptionInfo> GetSubscriptions(string dataType)
        {
            if (dataType == null)
                return subscriptions.Values.SelectMany(x => x).ToList();
            if(!subscriptions.ContainsKey(dataType))
                return new List<SubscriptionInfo>();
            return subscriptions[dataType];
        }

        public List<T> GetMany<T>(string filter, int limit)
        {
            var dataType = typeof(T).Name;
            if (!database.TryGetValue(dataType, out var collection))
                return new List<T>();
            if (string.IsNullOrEmpty(filter))
            {
                IEnumerable<object> items = collection.Values;
                if (limit >= 0)
                    items = items.Take(limit);
                return items.Cast<T>().ToList();
            }
            throw new NotSupportedException();
        }
    }
}
