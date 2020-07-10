namespace DataProcessing
{
    public class ExecutionResult
    {
        public ExecutionResult(bool isSuccess, bool isWorkDone, string summary)
        {
            IsSuccess = isSuccess;
            IsWorkDone = isWorkDone;
            Summary = summary;
        }

        public bool IsSuccess { get; }
        public bool IsWorkDone { get; }
        public string Summary { get; }
    }
}