using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Driver;

namespace DataAPI.Service
{
    public class ApiEventLogger
        : IEventLogger
    {
        private readonly RdDataMongoClient rdDataMongoClient;
        private readonly IMongoCollection<LogEntry> logEntryCollection;

        public ApiEventLogger(RdDataMongoClient rdDataMongoClient)
        {
            this.rdDataMongoClient = rdDataMongoClient;
            logEntryCollection = rdDataMongoClient.BackendDatabase.GetCollection<LogEntry>(nameof(LogEntry));
        }

        public void Log(LogEntry logEntry)
        {
            logEntryCollection.InsertOne(logEntry);
        }

        public void Log(LogLevel logLevel, string message)
        {
            Log(new LogEntry(logLevel, message));
        }
    }
}
