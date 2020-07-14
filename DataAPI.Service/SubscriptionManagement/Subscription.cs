using System;
using System.Collections.Generic;
using DataAPI.DataStructures.DataSubscription;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.SubscriptionManagement
{
    public class Subscription
    {
        [JsonConstructor]
        public Subscription(
            string id,
            string username,
            string dataType,
            List<DataModificationType> modificationTypes,
            string filter)
        {
            Id = id;
            Username = username;
            DataType = dataType;
            ModificationTypes = modificationTypes;
            Filter = filter;
        }

        public Subscription(
            string username,
            string dataType,
            List<DataModificationType> modificationTypes,
            string filter)
            : this(Guid.NewGuid().ToString(), username, dataType, modificationTypes, filter)
        {
        }

        [BsonId]
        public string Id { get; private set; }
        public string Username { get; private set; }
        public string DataType { get; private set; }
        public List<DataModificationType> ModificationTypes { get; private set; }
        public string Filter { get; private set; }
    }
}
