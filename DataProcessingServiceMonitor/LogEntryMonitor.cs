using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.Exceptions;
using DataProcessing.Logging;

namespace DataProcessingServiceMonitor
{
    public class LogEntryMonitor
    {
        private readonly IDataApiClient dataApiClient;
        
        private CancellationTokenSource cancellationTokenSource;
        private Task monitoringTask;
        private readonly EventWaitHandle manualWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        public LogEntryMonitor(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            
        }

        private bool isMonitoring;
        public bool IsMonitoring
        {
            get => isMonitoring;
            private set
            {
                if(value == isMonitoring)
                    return;
                isMonitoring = value;
                MonitoringStateChanged?.Invoke(this, isMonitoring);
                if (!isMonitoring)
                    IsHistoryLoaded = false;
            }
        }

        private bool isHistoryLoaded;
        public bool IsHistoryLoaded
        {
            get => isHistoryLoaded;
            private set
            {
                if(value == isHistoryLoaded)
                    return;
                isHistoryLoaded = value;
                if(isHistoryLoaded)
                    HistoricLogEntriesLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StartMonitoring()
        {
            if(IsMonitoring)
                return;
            IsMonitoring = true;
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            monitoringTask = Task.Factory.StartNew(
                async () => await MonitorForLogEntries(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Unwrap();
        }

        public void StopMonitoring()
        {
            if(!IsMonitoring)
                return;

            cancellationTokenSource?.Cancel();
            monitoringTask?.Wait();

            IsMonitoring = false;
        }

        public void PollNow()
        {
            manualWaitHandle.Set();
        }

        private async Task MonitorForLogEntries(CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    await dataApiClient.SubscribeAsync(
                        nameof(DataProcessingServiceLog),
                        new[] {DataModificationType.Created});
                }
                catch (ApiException apiException)
                {
                    if (apiException.StatusCode != HttpStatusCode.Conflict)
                        throw;
                }

                var allLogEntries = await dataApiClient.GetManyAsync<DataProcessingServiceLog>(
                    $"SubmissionTimeUtc > '{DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)):yyyy-MM-dd HH:mm:ss}'");
                allLogEntries.ForEach(logEntry => NewLogEntry?.Invoke(this, logEntry));
                IsHistoryLoaded = true;
                var fullLoadTimestamp = allLogEntries.Any()
                    ? allLogEntries.Select(logEntry => logEntry.Timestamp).Max()
                    : DateTime.MinValue;
                var pollInterval = TimeSpan.FromSeconds(10);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var dataNotifications = await dataApiClient.GetSubscribedObjects(
                        nameof(DataProcessingServiceLog));
                    var newLogEntries = new List<DataProcessingServiceLog>();
                    foreach (var dataNotification in dataNotifications)
                    {
                        var dataObjectExists = await dataApiClient.ExistsAsync<DataProcessingServiceLog>(
                            dataNotification.DataObjectId);
                        if(!dataObjectExists)
                            continue;
                        var logEntry = await dataApiClient.GetAsync<DataProcessingServiceLog>(
                            dataNotification.DataObjectId);
                        newLogEntries.Add(logEntry);
                    }

                    foreach (var newLogEntry in newLogEntries.OrderBy(logEntry => logEntry.Timestamp))
                    {
                        if (newLogEntry.Timestamp <= fullLoadTimestamp) 
                            continue;
                        NewLogEntry?.Invoke(this, newLogEntry);
                    }

                    foreach (var dataNotification in dataNotifications)
                    {
                        await dataApiClient.DeleteNotificationAsync(dataNotification.Id);
                    }

                    WaitHandle.WaitAny(new[] {cancellationToken.WaitHandle, manualWaitHandle}, pollInterval);
                }
            }
            finally
            {
                await dataApiClient.UnsubscribeAllAsync(nameof(DataProcessingServiceLog));
                IsMonitoring = false;
            }
        }

        public event EventHandler<DataProcessingServiceLog> NewLogEntry;
        public event EventHandler<bool> MonitoringStateChanged;
        public event EventHandler HistoricLogEntriesLoaded;
    }
}
