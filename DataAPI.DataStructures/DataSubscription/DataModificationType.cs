using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.DataSubscription
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataModificationType
    {
        Unknown = 0, // For error check only
        Created = 1,
        Replaced = 2,
        Deleted = 3
    }
}