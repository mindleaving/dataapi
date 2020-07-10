namespace DataProcessing.Objects
{
    public class PostponedProcessorResult : IProcessorResult
    {
        public PostponedProcessorResult(PostponedProcessingObject postponedObject)
        {
            PostponedObject = postponedObject;
        }

        public ProcessingStatus Status { get; } = ProcessingStatus.Postponed;

        public PostponedProcessingObject PostponedObject { get; }
    }
}