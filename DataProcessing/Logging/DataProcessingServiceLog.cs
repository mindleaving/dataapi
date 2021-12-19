using System;
using System.Threading;
using DataAPI.DataStructures;
using Newtonsoft.Json;

namespace DataProcessing.Logging
{
    public class DataProcessingServiceLog : IId
    {
        [JsonConstructor]
        private DataProcessingServiceLog(
            string id,
            string message,
            DateTime timestamp)
        {
            Id = id;
            Message = message;
            Timestamp = timestamp;
        }

        private static long LogCounter;
        public DataProcessingServiceLog(string message, ILogEntryDetails details)
        {
            Timestamp = DateTime.UtcNow;
            var logCount = Interlocked.Increment(ref LogCounter);
            Id = $"{Timestamp:yyyy-MM-dd_HHmmss}_{logCount:D6}";

            Message = message;
            Details = details;
        }

        public string Id { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        [JsonConverter(typeof(LogEntryDetailsJsonConverter))]
        public ILogEntryDetails Details { get; set; }
    }
}