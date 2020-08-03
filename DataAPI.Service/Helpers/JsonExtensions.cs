using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Helpers
{
    public static class JsonExtensions
    {
        public static JObject EnsureParent(this JObject jObject, string propertyPath)
        {
            var splittedPath = propertyPath.Split('.');
            var lastObject = jObject;
            for (var index = 0; index < splittedPath.Length-1; index++)
            {
                var parentPropertyName = splittedPath[index];
                if(!lastObject.ContainsKey(parentPropertyName))
                    lastObject.Add(parentPropertyName, new JObject());
                lastObject = (JObject)lastObject[parentPropertyName];
            }
            return lastObject;
        }
    }
}
