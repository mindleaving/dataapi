using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Physics;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Logging;
using DataProcessing.Objects;

namespace DataProcessing
{
    public class ProcessorRunner
    {
        private readonly IDataApiClient dataApiClient;
        private readonly IDataProcessingServiceLogger dataProcessingServiceLogger;

        public ProcessorRunner(IDataApiClient dataApiClient, IDataProcessingServiceLogger dataProcessingServiceLogger)
        {
            this.dataApiClient = dataApiClient;
            this.dataProcessingServiceLogger = dataProcessingServiceLogger;
        }

        public async Task ProcessObject(DataModificationType modificationType, string dataType, string inputId, string inputObjectJson, IProcessor typeProcessor)
        {
            //await LogExecutionStarting(typeProcessor);
            var stopWatch = Stopwatch.StartNew();
            bool isSuccess;
            var isWorkDone = true;
            string summary;
            string outputTypeName = null;
            try
            {
                var processorResult = await typeProcessor.Process(modificationType, dataType, inputId, inputObjectJson);
                if (processorResult.Status == ProcessingStatus.NotInterested)
                {
                    return;
                }
                else if (processorResult.Status == ProcessingStatus.Postponed)
                {
                    var postponedProcessorResult = (PostponedProcessorResult) processorResult;
                    await dataApiClient.InsertAsync(postponedProcessorResult.PostponedObject);
                    return;
                }
                else if (processorResult.Status == ProcessingStatus.Error)
                {
                    isSuccess = false;
                    var errorProcessorResult = (ErrorProcessorResult) processorResult;
                    summary = errorProcessorResult.ErrorMessage;
                }
                else if (processorResult.Status == ProcessingStatus.Success)
                {
                    isSuccess = true;
                    var successProcessorResult = (SuccessProcessorResult) processorResult;
                    foreach (var obj in successProcessorResult.Objects)
                    {
                        await StoreResult(obj);
                    }

                    summary = successProcessorResult.Summary;
                }
                else
                {
                    throw new InvalidEnumArgumentException($"Invalid enum value '{processorResult.Status}' for {nameof(ProcessingStatus)}");
                }
            }
            catch (Exception e)
            {
                isSuccess = false;
                isWorkDone = false;
                summary = e.InnermostException().Message;
                if (string.IsNullOrWhiteSpace(summary))
                    summary = e.Message;
            }

            stopWatch.Stop();
            await LogExecutionFinished(
                typeProcessor,
                outputTypeName,
                inputId,
                summary,
                isSuccess,
                isWorkDone,
                stopWatch);
        }

        public async Task LogExecutionStarting(IProcessor processor)
        {
            var logEntry = new DataProcessingServiceLog(
                $"Starting execution of processor '{processor.DisplayName}'",
                new ExecutionStartLogEntryDetails(
                    DataProcessingServiceExecutorType.Processor,
                    processor.DisplayName,
                    processor.InputTypes.Count == 1 ? processor.InputTypes.First() : "Multiple",
                    (processor as ISingleOutputProcessor)?.OutputTypeName ?? "Multiple"));
            await dataProcessingServiceLogger.Log(logEntry);
        }

        public async Task LogExecutionFinished(
            IProcessor processor,
            string outputTypeName,
            string inputId,
            string summary,
            bool isSuccess,
            bool isWorkDone,
            Stopwatch stopWatch)
        {
            var log = new DataProcessingServiceLog(
                summary,
                new ExecutionSummaryLogEntryDetails(
                    DataProcessingServiceExecutorType.Processor,
                    processor.DisplayName,
                    stopWatch.Elapsed.TotalSeconds.To(Unit.Second),
                    !isSuccess,
                    isWorkDone,
                    inputId,
                    processor.InputTypes.Count == 1 ? processor.InputTypes.First() : "Multiple",
                    outputTypeName));
            await dataProcessingServiceLogger.Log(log);
        }

        public async Task StoreResult(SerializedObject obj)
        {
            await dataApiClient.ReplaceAsync(
                obj.DataType,
                obj.Json,
                obj.Id);
        }
    }
}
