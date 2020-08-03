using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.DataSubscription;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class SubscriptionBody
    {
        [JsonConstructor]
        public SubscriptionBody(
            string dataType,
            List<DataModificationType> modificationTypes,
            string filter = null)
        {
            DataType = dataType;
            ModificationTypes = modificationTypes;
            Filter = filter;
        }

        [Required]
        public string DataType { get; }

        [Required]
        [MinLength(1)]
        public List<DataModificationType> ModificationTypes { get; }

        public string Filter { get; }
    }
}
