using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataServiceTargetType
    {
        Undefined = 0, // For validation only. Do not use intentionally.
        File = 1,
        Sql = 2
    }
}