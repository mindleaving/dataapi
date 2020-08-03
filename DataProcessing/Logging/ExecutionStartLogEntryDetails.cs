using Newtonsoft.Json;

namespace DataProcessing.Logging
{
    public class ExecutionStartLogEntryDetails : ILogEntryDetails
    {
        [JsonConstructor]
        public ExecutionStartLogEntryDetails(
            DataProcessingServiceExecutorType executorType, 
            string processorName,
            string inputTypeName = null,
            string outputTypeName = null)
        {
            ExecutorType = executorType;
            ProcessorName = processorName;
            if(executorType== DataProcessingServiceExecutorType.Processor)
            {
                InputTypeName = inputTypeName;
                OutputTypeName = outputTypeName;
            }
        }

        public string Type { get; private set; } = nameof(ExecutionStartLogEntryDetails);
        public DataProcessingServiceExecutorType ExecutorType { get; private set; }
        public string ProcessorName { get; private set; }
        public string InputTypeName { get; private set; }
        public string OutputTypeName { get; private set; }
    }
}