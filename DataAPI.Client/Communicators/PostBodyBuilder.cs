using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DataAPI.Client.Serialization;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Communicators
{
    public static class PostBodyBuilder
    {
        public static HttpContent ConstructBody(object bodyObject)
        {
            var json = ConfiguredJsonSerializer.Serialize(bodyObject);
            return ConstructBodyFromJson(json);
        }

        public static HttpContent ConstructBodyFromJObject(JObject jObject)
        {
            return ConstructBodyFromJson(jObject.ToString());
        }

        public static HttpContent ConstructBodyFromJson(string json)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=UTF-8");
            return content;
        }
    }
}
