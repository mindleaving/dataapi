using DataAPI.DataStructures;
using MongoDB.Bson.IO;

namespace DataAPI.Service
{
    public static class Conventions
    {
        public const string JsonContentType = "application/json";
        public const string OctetStreamContentType = "application/octet-stream";
        public const IdGeneratorType DefaultIdGenerator = IdGeneratorType.Guid;

        public static JsonWriterSettings OneLineMongoDbJsonSettings { get; } = new JsonWriterSettings
        {
            OutputMode = JsonOutputMode.Strict,
            NewLineChars = "",
            Indent = false
        };
    }
}
