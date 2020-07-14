using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.UserManagement
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        Viewer = 1,
        DataProducer = 2,
        Analyst = 3,
        UserManager = 4,
        Admin = 99
    }
}
