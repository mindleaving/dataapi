using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataProcessing.Objects
{
    public class SuccessProcessorResult : IProcessorResult
    {
        [JsonConstructor]
        public SuccessProcessorResult(
            string summary,
            bool isWorkDone,
            IList<SerializedObject> objects = null)
        {
            Summary = summary;
            IsWorkDone = isWorkDone;
            Objects = objects ?? new List<SerializedObject>();
        }

        public ProcessingStatus Status { get; } = ProcessingStatus.Success;
        public string Summary { get; }
        public bool IsWorkDone { get; }
        public IList<SerializedObject> Objects { get; }
    }
}
