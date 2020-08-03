using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.DataIo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortDirection
    {
        Undefined = 0,
        Ascending = 1,
        Descending = -1
    }
}