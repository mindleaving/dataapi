using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Validation
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ComparisonReferenceType
    {
        Static,
        OtherProperty
    }
}