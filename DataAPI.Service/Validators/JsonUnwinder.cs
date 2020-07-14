using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.Validators
{
    public static class JsonUnwinder
    {
        public static IEnumerable<JToken> Unwind(JToken jToken, string propertyPath)
        {
            var stopUnwinding = string.IsNullOrEmpty(propertyPath);
            if (stopUnwinding)
            {
                yield return jToken;
                yield break;
            }
            if (jToken.Type == JTokenType.Array)
            {
                foreach (var item in jToken.Children())
                {
                    foreach (var unwoundItem in Unwind(item, propertyPath))
                    {
                        yield return unwoundItem;
                    }
                }
            }
            else
            {
                var splittedPropertyPath = propertyPath.Split(new []{'.'}, 2);
                var propertyName = splittedPropertyPath[0];
                var remainingPath = splittedPropertyPath.Length > 1 ? splittedPropertyPath[1] : null;
                var hasMatchingProperty = jToken.HasValues && ((JObject) jToken).ContainsKey(propertyName);
                if (!hasMatchingProperty)
                {
                    yield return null;
                    yield break;
                }
                var propertyObject = jToken[propertyName];
                foreach (var unwoundItem in Unwind(propertyObject, remainingPath))
                {
                    yield return unwoundItem;
                }
            }
        }
    }
}
