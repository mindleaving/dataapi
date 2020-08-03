using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Physics;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.Exceptions;
using DataProcessing.Logging;

namespace DataProcessing
{
    /// <summary>
    /// Gets data from Data API, passes it to processors and returns result back to Data API
    /// </summary>
    public class Distributor : IDisposable
    {
        private readonly IDataApiClient dataApiClient;
        private readonly ProcessorDatabase processorDatabase;
        private Task runTask;
        private CancellationTokenSource cancellationTokenSource;
        private readonly AutoResetEvent pollNowEventHandle = new AutoResetEvent(false);
        private readonly IDataProcessingServiceLogger dataProcessingServiceLogger;
        private readonly ProcessorRunner processorRunner;

        public Distributor(
            IDataApiClient dataApiClient,
            ProcessorDatabase processorDatabase,
            IDataProcessingServiceLogger dataProcessingServiceLogger)
        {
            this.dataApiClient = dataApiClient;
            this.processorDatabase = processorDatabase;
            this.dataProcessingServiceLogger = dataProcessingServiceLogger;
            processorRunner = new ProcessorRunner(dataApiClient, dataProcessingServiceLogger);
        }

        public bool IsRunning { get; private set; }
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);

        public void Start()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            EnsureDataSubscription();
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            runTask = Task.Factory.StartNew(
                async () => await Run(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Unwrap();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            cancellationTokenSource.Cancel();
            try
            {
                runTask.Wait();
            }
            finally
            {
                IsRunning = false;
            }
        }

        public void PollNow()
        {
            pollNowEventHandle.Set();
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog($"{nameof(Distributor)} started", null));
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog(
                    $"{nameof(Distributor)} running for types '{processorDatabase.InputTypes.Aggregate((a, b) => a + ", " + b)}'", null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (dataApiClient.IsAvailable())
                    {
                        if (!dataApiClient.IsLoggedIn && !dataApiClient.RetryLogin().IsAuthenticated)
                        {
                            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(60));
                            continue;
                        }

                        try
                        {
                            var subscriptionNotifications = await dataApiClient.GetSubscribedObjects();
                            foreach (var subscriptionNotification in subscriptionNotifications)
                            {
                                if(cancellationToken.IsCancellationRequested)
                                    break;
                                var dataType = subscriptionNotification.DataType;
                                if (subscriptionNotification.ModificationType != DataModificationType.Deleted)
                                {
                                    var exists = await dataApiClient.ExistsAsync(
                                        dataType,
                                        subscriptionNotification.DataObjectId);
                                    if (!exists)
                                    {
                                        await MarkAsProcessed(subscriptionNotification.Id);
                                        continue;
                                    }
                                }

                                try
                                {
                                    var typeProcessors = processorDatabase.GetForType(dataType);
                                    switch (subscriptionNotification.ModificationType)
                                    {
                                        case DataModificationType.Created:
                                        case DataModificationType.Replaced:
                                            var typeObject = await LoadObject(subscriptionNotification);
                                            await ApplyProcessorsToObject(
                                                subscriptionNotification.ModificationType,
                                                dataType,
                                                subscriptionNotification.DataObjectId,
                                                typeObject,
                                                typeProcessors);
                                            break;
                                        case DataModificationType.Deleted:
                                            await ApplyProcessorsToObject(
                                                subscriptionNotification.ModificationType,
                                                dataType,
                                                subscriptionNotification.DataObjectId,
                                                null,
                                                typeProcessors);
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    await MarkAsProcessed(subscriptionNotification.Id);
                                }
                                catch (Exception e)
                                {
                                    var logEntry = new DataProcessingServiceLog(
                                        $"Processing of '{dataType}' with ID '{subscriptionNotification.DataObjectId}' failed: {e.Message}",
                                        new ExecutionSummaryLogEntryDetails(
                                            DataProcessingServiceExecutorType.Processor,
                                            "Unknown",
                                            0.To(Unit.Second),
                                            isError: true,
                                            isWorkDone: false,
                                            inputDataObjectId: subscriptionNotification.DataObjectId,
                                            inputTypeName: dataType));
                                    await dataProcessingServiceLogger.Log(logEntry);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            var logEntry = new DataProcessingServiceLog(
                                $"Processing of subscription notifications failed: {e.InnermostException().Message}",
                                new CrashLogEntryDetails(nameof(Distributor), e.InnermostException().Message));
                            await dataProcessingServiceLogger.Log(logEntry);
                        }
                    }

                    pollNowEventHandle.Reset();
                    WaitHandle.WaitAny(new[] {cancellationToken.WaitHandle, pollNowEventHandle}, PollInterval);
                }
            }
            catch (Exception e)
            {
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog(
                    $"Distributor crashed: {e.InnermostException().Message}",
                    new CrashLogEntryDetails(nameof(Distributor), e.InnermostException().Message)));
            }

            try
            {
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog($"{nameof(Distributor)} stopped", null));
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private void EnsureDataSubscription()
        {
            var subscriptionTasks = new List<Task>();
            processorDatabase.InputTypeAdded += OnInputTypeAdded;
            processorDatabase.InputTypeRemoved += OnInputTypeRemoved;
            foreach (var inputTypeName in processorDatabase.InputTypes)
            {
                var subscriptionTask = Task.Run(async () => await SubscribeToDataType(inputTypeName));
                subscriptionTasks.Add(subscriptionTask);
            }

            Task.WaitAll(subscriptionTasks.ToArray());
        }

        private async void OnInputTypeRemoved(object sender, InputTypeRegistrationEventArgs e)
        {
            await UnsubscribeFromDataType(e.DataType);
        }

        private async void OnInputTypeAdded(object sender, InputTypeRegistrationEventArgs e)
        {
            await SubscribeToDataType(e.DataType);
        }

        private async Task SubscribeToDataType(string dataType)
        {
            var modificationTypes = EnumExtensions
                .GetValues<DataModificationType>()
                .Except(new[] {DataModificationType.Unknown})
                .ToList();
            try
            {
                await dataApiClient.SubscribeAsync(dataType, modificationTypes);
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode == HttpStatusCode.Conflict)
                {
                    return;
                }
                throw;
            }
        }

        private async Task UnsubscribeFromDataType(string dataType)
        {
            await dataApiClient.UnsubscribeAllAsync(dataType);
        }

        private async Task<string> LoadObject(SubscriptionNotification subscriptionNotification)
        {
            return await dataApiClient.GetAsync(
                subscriptionNotification.DataType,
                subscriptionNotification.DataObjectId);
        }

        private async Task MarkAsProcessed(string notificationId)
        {
            await dataApiClient.DeleteNotificationAsync(notificationId);
        }

        private async Task ApplyProcessorsToObject(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputObjectJson,
            IEnumerable<IProcessor> typeProcessors)
        {
            foreach (var typeProcessor in typeProcessors)
            {
                await processorRunner.ProcessObject(modificationType, dataType, inputId, inputObjectJson, typeProcessor);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
