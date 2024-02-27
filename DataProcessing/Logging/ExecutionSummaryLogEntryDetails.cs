using System;
using Commons.Physics;

namespace DataProcessing.Logging
{
    public class ExecutionSummaryLogEntryDetails : ILogEntryDetails
    {
        public ExecutionSummaryLogEntryDetails() {}
        public ExecutionSummaryLogEntryDetails(
            DataProcessingServiceExecutorType executorType,
            string processorName,
            UnitValue executionTime,
            bool isError,
            bool isWorkDone,
            string inputDataObjectId = null,
            string inputTypeName = null,
            string outputTypeName = null)
        {
            ExecutorType = executorType;
            ProcessorName = processorName ?? throw new ArgumentNullException(nameof(processorName));
            ExecutionTime = executionTime ?? throw new ArgumentNullException(nameof(executionTime));
            IsError = isError;
            IsWorkDone = isWorkDone;
            if (ExecutorType == DataProcessingServiceExecutorType.Processor)
            {
                InputDataObjectId = inputDataObjectId ?? throw new ArgumentNullException(nameof(inputDataObjectId));
                InputTypeName = inputTypeName ?? throw new ArgumentNullException(nameof(inputTypeName));
                OutputTypeName = outputTypeName; // Can be null for IOneToNoneProcessor
            }
        }

        public string Type { get; private set; } = nameof(ExecutionSummaryLogEntryDetails);
        public DataProcessingServiceExecutorType ExecutorType { get; private set; }
        public string ProcessorName { get; private set; }
        public UnitValue ExecutionTime { get; private set; }
        public bool IsError { get; private set; }
        public bool IsWorkDone { get; private set; }
        public string InputDataObjectId { get; private set; }
        public string InputTypeName { get; private set; }
        public string OutputTypeName { get; private set; }
    }
}