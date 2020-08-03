using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DataAPI.Client.Serialization
{
    internal static class ConfiguredJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Error = OnError,
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
            ErrorOccurred?.Invoke(null, e);
        }

        public static event EventHandler<ErrorEventArgs> ErrorOccurred;
    }
}
