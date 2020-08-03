using System.Collections.Generic;
using DataProcessing.Models;
using Newtonsoft.Json.Linq;

namespace DataProcessing.DataServices
{
    public class KeyValuePairExtractor
    {
        public List<KeyValuePair<string, string>> Extract(JObject jObject, List<DataServiceDefinition.Field> fields)
        {
            var kvps = new List<KeyValuePair<string, string>>();
            foreach (var field in fields)
            {
                var key = field.As;
                var fieldPath = StripDataPrefix(field.Path);
                var value = jObject.SelectToken(fieldPath)?.ToString();
                kvps.Add(new KeyValuePair<string, string>(key, value));
            }

            return kvps;
        }

        private static string StripDataPrefix(string fieldPath)
        {
            if (fieldPath.StartsWith("Data."))
                return fieldPath.Substring("Data.".Length);
            return fieldPath;
        }
    }
}
