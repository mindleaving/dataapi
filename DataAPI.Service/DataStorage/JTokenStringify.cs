using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace DataAPI.Service.DataStorage
{
    public static class JTokenStringify
    {
        public static string Stringify(JToken jToken)
        {
            if (jToken is JValue jValue)
            {
                if (jValue.Type == JTokenType.Date)
                    return jValue.Value<DateTime>().ToString("yyyy-MM-dd HH:mm:ss");
                return jValue.ToString(CultureInfo.InvariantCulture);
            }
            return jToken.ToString();
        }
    }
}
