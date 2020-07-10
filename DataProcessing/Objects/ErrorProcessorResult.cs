namespace DataProcessing.Objects
{
    public class ErrorProcessorResult : IProcessorResult
    {
        public ErrorProcessorResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public ProcessingStatus Status { get; } = ProcessingStatus.Error;
        public string ErrorMessage { get; }
    }
}