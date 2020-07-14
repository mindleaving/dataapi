using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultFormat
    {
        Undefined = 0,  // Should never be used, except for checking for invalid serialization and other coding errors
        Json = 1,
        Csv = 2
    }
}