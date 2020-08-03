using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;

namespace DataProcessing.Logging
{
    public class LogTruncationTask : IPeriodicTask
    {
        private readonly IDataApiClient dataApiClient;
        
        private readonly TimeSpan logPreservationTimeSpan;

        public LogTruncationTask(
            IDataApiClient dataApiClient,
            TimeSpan logPreservationTimeSpan)
        {
            this.dataApiClient = dataApiClient;
            this.logPreservationTimeSpan = logPreservationTimeSpan;
        }

        public string DisplayName { get; } = nameof(LogTruncationTask);
        public TimeSpan Period { get; } = TimeSpan.FromDays(1);

        public async Task<ExecutionResult> Action(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var cutoffDateTime = now - logPreservationTimeSpan;
            var query = "SELECT _id "
                        + $"FROM {DataApiClient.GetCollectionName<DataProcessingServiceLog>()} "
                        + $"WHERE SubmissionTimeUtc < '{cutoffDateTime:yyyy-MM-dd HH:mm:ss}'";
            var resultStream = await dataApiClient.SearchAsync(query, ResultFormat.Json);
            var ids = (await resultStream.ReadAllSearchResultsAsync()).Select(jObject => jObject.Value<string>("_id"));
            var deletionCount = 0;
            foreach (var id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                dataApiClient.DeleteAsync<DataProcessingServiceLog>(id).Wait(cancellationToken);
                deletionCount++;
            }
            return new ExecutionResult(true, deletionCount > 0, 
                $"Deleted all {nameof(DataProcessingServiceLog)} before '{cutoffDateTime:yyyy-MM-dd HH:mm:ss}' (count: {deletionCount})");
        }
    }
}
