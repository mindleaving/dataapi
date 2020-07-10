using System;
using Commons.Physics;
using DataProcessing;
using DataProcessing.Logging;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class LogEntryViewModel
    {
        public LogEntryViewModel(DataProcessingServiceLog logEntry)
        {
            Model = logEntry;
            switch (logEntry.Details)
            {
                case ExecutionStartLogEntryDetails executionStartLogEntryDetails:
                    ExecutorType = executionStartLogEntryDetails.ExecutorType;
                    ProcessorName = executionStartLogEntryDetails.ProcessorName;
                    InputTypeName = executionStartLogEntryDetails.InputTypeName;
                    OutputTypeName = executionStartLogEntryDetails.OutputTypeName;
                    break;
                case ExecutionSummaryLogEntryDetails executionSummaryLogEntryDetails:
                    ExecutorType = executionSummaryLogEntryDetails.ExecutorType;
                    ProcessorName = executionSummaryLogEntryDetails.ProcessorName;
                    InputTypeName = executionSummaryLogEntryDetails.InputTypeName;
                    OutputTypeName = executionSummaryLogEntryDetails.OutputTypeName;
                    ExecutionTime = executionSummaryLogEntryDetails.ExecutionTime;
                    IsError = executionSummaryLogEntryDetails.IsError;
                    break;
                case CrashLogEntryDetails crashLogEntryDetails:
                    IsError = true;
                    break;
            }
        }


        public DataProcessingServiceLog Model { get; }
        public string Message => Model.Message;
        public DateTime Timestamp => Model.Timestamp;
        public string DetailsType => Model.Details?.Type ?? "General";
        public DataProcessingServiceExecutorType? ExecutorType { get; }
        public string ProcessorName { get; }
        public string InputTypeName { get; }
        public string OutputTypeName { get; }
        public UnitValue ExecutionTime { get; }
        public bool IsError { get; }
    }
}
