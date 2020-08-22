using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataAPI.DataStructures.Validation
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValidatorType
    {
        PythonScript = 1,
        Exe = 2,
        JsonSchema = 3,
        TextRules = 4
    }
}