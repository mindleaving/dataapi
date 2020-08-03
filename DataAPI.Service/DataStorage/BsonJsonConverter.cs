using System;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public class BsonJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BsonDocument);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bsonDocument = value as BsonDocument;
            var json = DataEncoder.DecodeToJson(bsonDocument);
            writer.WriteRawValue(json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            //var jObject = reader.Value as JObject;
            return DataEncoder.Encode(jObject?.ToString());
        }
    }
}
