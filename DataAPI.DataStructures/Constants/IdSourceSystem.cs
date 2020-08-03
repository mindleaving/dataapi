using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IdSourceSystem
    {
        SelfAssigned = 0
    }
}