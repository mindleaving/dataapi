using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CrudOperationEnum
    {
        Create,
        Read,
        Update, 
        Delete
    }
}
