using DataAPI.Service.Objects;

namespace DataAPI.Service
{
    public interface IEventLogger
    {
        void Log(LogEntry logEntry);
        void Log(LogLevel logLevel, string message);
    }
}