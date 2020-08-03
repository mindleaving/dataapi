namespace DataProcessing.Logging
{
    public class CrashLogEntryDetails : ILogEntryDetails
    {
        public CrashLogEntryDetails(string crashedEntity, string exceptionMessage)
        {
            CrashedEntity = crashedEntity;
            ExceptionMessage = exceptionMessage;
        }

        public string Type { get; } = nameof(CrashLogEntryDetails);
        public string CrashedEntity { get; private set; }
        public string ExceptionMessage { get; private set; }
    }
}
