using System;
using DataAPI.DataStructures.DataSubscription;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.SubscriptionManagement
{
    public class DataNotification
    {
        [JsonConstructor]
        private DataNotification(
            string id,
            string username, 
            string dataType,
            string dataObjectId,
            DataModificationType modificationType)
        {
            Id = id;
            Username = username;
            DataType = dataType;
            DataObjectId = dataObjectId;
            ModificationType = modificationType;
        }

        public DataNotification(
            string username,
            string dataType,
            string dataObjectId,
            DataModificationType modificationType)
            : this(Guid.NewGuid().ToString(), username, dataType, dataObjectId, modificationType)
        {
        }

        [BsonId]
        public string Id { get; private set; }
        public string Username { get; private set; }
        public string DataType { get; private set; }
        public string DataObjectId { get; private set; }
        public DataModificationType ModificationType { get; private set; }
    }
}
