using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.Exceptions;
using DataProcessing.Logging;
using DataProcessing.Objects;
using Newtonsoft.Json;

namespace DataProcessing.ProcessingPostponing
{
    /// <summary>
    /// NOTE: This runner works in tandem with <see cref="PostponedProcessingObjectUpdateProcessor"/>.
    /// This runner is responsible for executing processors for which all conditions are met (no missing data) and remove the postponed processing object.
    /// <see cref="PostponedProcessingObjectUpdateProcessor"/> is responsible for listening for new data and subscribing/unsubscribing to the relevant data types
    /// </summary>
    public class PostponedProcessingRunner : ProcessorBase
    {
        private Task runnerTask;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IDataApiClient dataApiClient;
        private readonly IDataProcessingServiceLogger dataProcessingServiceLogger;
        private readonly Dictionary<string, IProcessor> processors;
        private readonly ProcessorRunner processorRunner;
        private readonly Dictionary<string, PostponedProcessingObject> postponedObjects = new Dictionary<string, PostponedProcessingObject>();
        private readonly AutoResetEvent pollNowEventHandle = new AutoResetEvent(false);

        public PostponedProcessingRunner(
            IDataApiClient dataApiClient,
            IEnumerable<IProcessor> processors, 
            IDataProcessingServiceLogger dataProcessingServiceLogger)
            : base(nameof(PostponedProcessingRunner), DataApiClient.GetCollectionName<PostponedProcessingObject>())
        {
            this.dataApiClient = dataApiClient;
            this.dataProcessingServiceLogger = dataProcessingServiceLogger;
            this.processors = processors.ToDictionary(x => x.DisplayName, x => x);
            processorRunner = new ProcessorRunner(dataApiClient, dataProcessingServiceLogger);
        }

        public bool IsRunning { get; private set; }
        public int PostponedObjectCount => postponedObjects.Count;

        public void Start()
        {
            if(IsRunning)
                return;

            IsRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            runnerTask = Task.Factory.StartNew(
                async () => await Run(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Unwrap();
        }

        public void Stop()
        {
            if(!IsRunning)
                return;

            cancellationTokenSource.Cancel();
            try
            {
                runnerTask.Wait();
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
            await dataProcessingServiceLogger.Log(new DataProcessingServiceLog($"{nameof(PostponedProcessingRunner)} started", null));

            await LoadPostponedObjects();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var noMoreAttemptsObjects = postponedObjects.Values.Where(x => x.RemainingAttempts <= 0).ToList();
                    foreach (var noMoreAttemptsObject in noMoreAttemptsObjects)
                    {
                        await RemovePostponedObject(noMoreAttemptsObject.Id);
                    }

                    var readyObjects = postponedObjects.Values.Where(x => !x.MissingData.Any()).ToList();
                    foreach (var readyObject in readyObjects)
                    {
                        var processingStatus = await RunPostponedObject(readyObject);
                        if (processingStatus != ProcessingStatus.Postponed)
                        {
                            await RemovePostponedObject(readyObject.Id);
                        }
                    }

                    var now = DateTime.UtcNow;
                    var expiredObjects = postponedObjects.Values.Where(x => now > x.LastAttempt + x.MaxWaitTime).ToList();
                    foreach (var expiredObject in expiredObjects)
                    {
                        var processingStatus = await RunPostponedObject(expiredObject);
                        if (processingStatus != ProcessingStatus.Postponed)
                        {
                            await RemovePostponedObject(expiredObject.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    await LogException(e);
                }

                WaitHandle.WaitAny(new[] {cancellationToken.WaitHandle, pollNowEventHandle}, TimeSpan.FromSeconds(10));
            }
        }

        private async Task LoadPostponedObjects()
        {
            var missingDataTypes = new HashSet<string>();
            foreach (var postponedObject in await dataApiClient.GetManyAsync<PostponedProcessingObject>())
            {
                postponedObjects[postponedObject.Id] = postponedObject;
                foreach (var dataReference in postponedObject.MissingData)
                {
                    if (missingDataTypes.Contains(dataReference.DataType))
                        continue;
                    missingDataTypes.Add(dataReference.DataType);
                }
            }
        }

        private async Task LogException(Exception e)
        {
            Console.WriteLine(e);
            await dataProcessingServiceLogger.Log(
                new DataProcessingServiceLog(
                    $"{nameof(PostponedProcessingRunner)} has encounted an exception: " + e.InnermostException().Message,
                    new CrashLogEntryDetails(
                        nameof(PostponedProcessingRunner),
                        e.ToString())));
        }

        private async Task RemovePostponedObject(string objectId)
        {
            postponedObjects.Remove(objectId);
            try
            {
                await dataApiClient.DeleteAsync<PostponedProcessingObject>(objectId);
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
        }

        private async Task<ProcessingStatus> RunPostponedObject(PostponedProcessingObject postponedObject)
        {
            var processor = processors[postponedObject.ProcessorName];
            var inputObjectJson = await dataApiClient.GetAsync(postponedObject.DataType, postponedObject.DataId);

            //await processorRunner.LogExecutionStarting(processor);
            bool isSuccess;
            bool isWorkDone;
            string summary;
            ProcessingStatus status;
            var stopWatch = Stopwatch.StartNew();
            try
            {
                var processorResult = await processor.Process(postponedObject.ModificationType, postponedObject.DataType, postponedObject.DataId, inputObjectJson);
                stopWatch.Stop();

                status = processorResult.Status;
                if (status == ProcessingStatus.NotInterested)
                {
                    return status;
                }
                else if (status == ProcessingStatus.Postponed)
                {
                    postponedObject.LastAttempt = DateTime.UtcNow;
                    postponedObject.RemainingAttempts--;
                    await dataApiClient.ReplaceAsync(postponedObject, postponedObject.Id); // Indirectly updated postponedObjects
                    return status;
                }
                else if (status == ProcessingStatus.Error)
                {
                    var errorProcessorResult = (ErrorProcessorResult) processorResult;
                    isSuccess = false;
                    isWorkDone = false;
                    summary = errorProcessorResult.ErrorMessage;
                }
                else if (status == ProcessingStatus.Success)
                {
                    var successProcessorResult = (SuccessProcessorResult)processorResult;
                    foreach (var obj in successProcessorResult.Objects)
                    {
                        await processorRunner.StoreResult(obj);
                    }
                    isSuccess = true;
                    isWorkDone = successProcessorResult.IsWorkDone;
                    summary = successProcessorResult.Summary;
                }
                else
                {
                    throw new InvalidEnumArgumentException($"Invalid enum value '{status}' for {nameof(ProcessingStatus)}");
                }
            }
            catch (Exception e)
            {
                isSuccess = false;
                isWorkDone = false;
                status = ProcessingStatus.Error;
                summary = e.InnermostException().Message;
                if (string.IsNullOrWhiteSpace(summary))
                    summary = e.Message;
            }
            await processorRunner.LogExecutionFinished(
                processor,
                (processor as ISingleOutputProcessor)?.OutputTypeName,
                postponedObject.DataId,
                summary,
                isSuccess,
                isWorkDone,
                stopWatch);
            return status;
        }

        public override Task<IProcessorResult> Process(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputObjectJson)
        {
            switch (modificationType)
            {
                case DataModificationType.Created:
                case DataModificationType.Replaced:
                    var postponedObject = JsonConvert.DeserializeObject<PostponedProcessingObject>(inputObjectJson);
                    if (!processors.ContainsKey(postponedObject.ProcessorName))
                        return Task.FromResult<IProcessorResult>(new NotInterestedProcessorResult());
                    postponedObjects[postponedObject.Id] = postponedObject;
                    break;
                case DataModificationType.Deleted:
                    postponedObjects.Remove(inputId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null);
            }
            return Task.FromResult<IProcessorResult>(new SuccessProcessorResult("Updated postponed object", true));
        }
    }
}
