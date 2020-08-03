using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataSubscription
{
    public class SubscriptionInfo
    {
        [JsonConstructor]
        public SubscriptionInfo(
            string id,
            string dataType,
            List<DataModificationType> modificationTypes,
            string filter)
        {
            Id = id;
            DataType = dataType;
            ModificationTypes = modificationTypes;
            Filter = filter;
        }

        public string Id { get; }
        public string DataType { get; }
        public List<DataModificationType> ModificationTypes { get; }
        public string Filter { get; }
    }
}
