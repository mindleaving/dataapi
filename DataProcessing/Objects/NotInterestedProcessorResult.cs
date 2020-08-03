namespace DataProcessing.Objects
{
    public class NotInterestedProcessorResult : IProcessorResult
    {
        public ProcessingStatus Status { get; } = ProcessingStatus.NotInterested;
    }
}