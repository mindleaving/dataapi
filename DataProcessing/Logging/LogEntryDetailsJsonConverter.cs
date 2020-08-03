using System;
using Newtonsoft.Json;

namespace DataProcessing.Logging
{
    public class LogEntryDetailsJsonConverter : JsonConverter<ILogEntryDetails>
    {
        public override void WriteJson(JsonWriter writer, ILogEntryDetails value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override ILogEntryDetails ReadJson(JsonReader reader, Type objectType, ILogEntryDetails existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (hasExistingValue)
                return existingValue;

            dynamic logEntryDetails = serializer.Deserialize(reader);
            if (logEntryDetails == null)
                return null;
            switch ((string)logEntryDetails.Type)
            {
                case nameof(ExecutionStartLogEntryDetails):
                    return JsonConvert.DeserializeObject<ExecutionStartLogEntryDetails>(logEntryDetails.ToString());
                case nameof(ExecutionSummaryLogEntryDetails):
                    return JsonConvert.DeserializeObject<ExecutionSummaryLogEntryDetails>(logEntryDetails.ToString());
                case nameof(CrashLogEntryDetails):
                    return JsonConvert.DeserializeObject<CrashLogEntryDetails>(logEntryDetails.ToString());
                default:
                    throw new NotSupportedException($"Log entry details of type '{logEntryDetails.Type}' is not supported");
            }
        }
    }
}