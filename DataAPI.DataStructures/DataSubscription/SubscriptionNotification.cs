using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataSubscription
{
    public class SubscriptionNotification
    {
        [JsonConstructor]
        public SubscriptionNotification(
            string id,
            string dataType,
            string dataObjectId,
            DataModificationType modificationType)
        {
            Id = id;
            DataType = dataType;
            DataObjectId = dataObjectId;
            ModificationType = modificationType;
        }

        public string Id { get; }
        public string DataType { get; }
        public string DataObjectId { get; }
        public DataModificationType ModificationType { get; }
    }
}
