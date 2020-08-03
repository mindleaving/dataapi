using System;
using System.Threading;
using MongoDB.Bson.Serialization.Attributes;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.Objects
{
    public class LogEntry
    {
        [BsonIgnore]
        private static long counter;

        [BsonConstructor]
        private LogEntry() {}
        public LogEntry(LogLevel severity, string message)
        {
            Timestamp = DateTime.UtcNow;
            Id = GetId();
            Severity = severity;
            Message = message;
        }

        [BsonId]
        public string Id { get; private set; } // Private setter necessary for MongoDB serialization (because no matching constructor for all properties?)
        public DateTime Timestamp { get; private set; } // Private setter necessary (see above)
        public LogLevel Severity { get; private set; } // Private setter necessary (see above)
        public string Message { get; private set; } // Private setter necessary (see above)

        private string GetId()
        {
            var localCounter = Interlocked.Increment(ref counter);
            return $"{Timestamp:yyyy-MM-dd_HHmmssfff}_{localCounter}_{MachineName.Name}";
        }
    }
}
