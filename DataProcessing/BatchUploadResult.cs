namespace DataProcessing
{
    public class BatchUploadResult
    {
        public BatchUploadResult(
            int objectCount,
            int successfulUploadCount,
            int failedUploadCount,
            int modifiedCount)
        {
            ObjectCount = objectCount;
            SuccessfulUploadCount = successfulUploadCount;
            FailedUploadCount = failedUploadCount;
            ModifiedCount = modifiedCount;
        }

        public int ObjectCount { get; }
        public int SuccessfulUploadCount { get; }
        public int FailedUploadCount { get; }
        public int ModifiedCount { get; }
    }
}