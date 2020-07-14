using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataCollectionProtocolParameterType
    {
        Text = 0,
        Number = 1,
        Date = 2,
        UnitValue = 3,
        DataType = 4
    }
}