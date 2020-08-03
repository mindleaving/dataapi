using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class GenericDataContainerTraversal
    {
        public const char PathDelimiter = '#';

        public static Dictionary<string, string> Traverse(GenericDataContainer container)
        {
            var jObject = JObject.Parse(DataEncoder.DecodeToJson(container.Data));
            var keyValuePairs = new Dictionary<string, string>
            {
                {"Id", container.Id},
                {"OriginalSubmitter", container.OriginalSubmitter},
                {"CreatedTimeUtc", container.CreatedTimeUtc?.ToString("yyyy-MM-dd HH:mm:ss")},
                {"Submitter", container.Submitter},
                {"SubmissionTimeUtc", container.SubmissionTimeUtc.ToString("yyyy-MM-dd HH:mm:ss")}
            };
            TraverseJObject(jObject, keyValuePairs, "Data");
            return keyValuePairs;
        }

        private static void TraverseJObject(JObject jObject, Dictionary<string, string> keyValuePairs, string parentPath)
        {
            foreach (var property in jObject.Properties())
            {
                var propertyPath = parentPath != null
                    ? parentPath + PathDelimiter + property.Name
                    : property.Name;
                if(property.Value.Type == JTokenType.Object)
                    TraverseJObject((JObject)property.Value, keyValuePairs, propertyPath);
                else
                    keyValuePairs.Add(propertyPath, JTokenStringify.Stringify(property.Value));
            }
        }
    }
}
