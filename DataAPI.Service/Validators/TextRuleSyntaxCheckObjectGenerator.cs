using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public static class TextRuleSyntaxCheckObjectGenerator
    {
        public static JObject ConstructJObjectWithPropertyPath(params string[] propertyNames)
        {
            var jObject = new JObject();
            foreach (var propertyName in propertyNames)
            {
                var dotSplittedPropertyName = propertyName.Split('.');
                var currentLevelObject = jObject;
                for (var levelIdx = 0; levelIdx < dotSplittedPropertyName.Length; levelIdx++)
                {
                    var partialPropertyName = dotSplittedPropertyName[levelIdx];
                    if (levelIdx == dotSplittedPropertyName.Length - 1)
                    {
                        if(!currentLevelObject.ContainsKey(partialPropertyName))
                            currentLevelObject.Add(new JProperty(partialPropertyName, "Test"));
                    }
                    else
                    {
                        JObject nextLevelObject;
                        if(!currentLevelObject.ContainsKey(partialPropertyName))
                        {
                            nextLevelObject = new JObject();
                            currentLevelObject.Add(new JProperty(partialPropertyName, nextLevelObject));
                        }
                        else
                        {
                            nextLevelObject = (JObject)currentLevelObject[partialPropertyName];
                        }
                        currentLevelObject = nextLevelObject;
                    }
                }
            }
            return jObject;
        }
    }
}
