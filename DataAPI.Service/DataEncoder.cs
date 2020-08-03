using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service
{
    public static class DataEncoder
    {
        public static BsonDocument Encode(object obj)
        {
            if (obj is JObject jObject)
                return Encode(jObject.ToString());
            return Encode(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }
        public static BsonDocument Encode(string json)
        {
            return BsonDocument.Parse(json);
        }

        public static string DecodeToJson(BsonDocument bsonDocument)
        {
            return bsonDocument?.ToJson(Conventions.OneLineMongoDbJsonSettings);
        }
    }
}
