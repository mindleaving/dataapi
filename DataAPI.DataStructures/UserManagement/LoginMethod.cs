using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.UserManagement
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoginMethod
    {
        ActiveDirectory,
        JsonWebToken
    }
}