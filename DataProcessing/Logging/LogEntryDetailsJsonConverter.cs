using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataProcessing.Logging
{
    public class LogEntryDetailsJsonConverter : JsonConverter<ILogEntryDetails>
    {
        public override void WriteJson(JsonWriter writer, ILogEntryDetails value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override ILogEntryDetails ReadJson(
            JsonReader reader, 
            Type objectType, 
            ILogEntryDetails existingValue, 
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var logEntryType = jObject["Type"]!.Value<string>();
            ILogEntryDetails logEntryDetails = logEntryType switch
            {
                nameof(ExecutionStartLogEntryDetails) => new ExecutionStartLogEntryDetails(),
                nameof(ExecutionSummaryLogEntryDetails) => new ExecutionSummaryLogEntryDetails(),
                nameof(CrashLogEntryDetails) => new CrashLogEntryDetails(),
                _ => throw new NotSupportedException($"Log entry details of type '{logEntryType}' is not supported")
            };
            serializer.Populate(jObject.CreateReader(), logEntryDetails);
            return logEntryDetails;
        }
    }
}