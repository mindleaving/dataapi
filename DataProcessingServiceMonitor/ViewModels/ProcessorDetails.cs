using Commons.Physics;
using DataProcessing;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class ProcessorDetails : IExecutorDetails
    {
        public ProcessorDetails(string name, string inputTypeName, string outputTypeName)
        {
            Name = name;
            InputTypeName = inputTypeName;
            OutputTypeName = outputTypeName ?? "None";
        }

        public string Name { get; }
        public DataProcessingServiceExecutorType ExecutorType { get; set; }
        public string InputTypeName { get; }
        public string OutputTypeName { get; }
        public UnitValue ExecutionTimeLast24Hours { get; set; }
    }
}