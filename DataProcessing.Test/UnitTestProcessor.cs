using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.DomainModels;
using DataProcessing.Objects;
using Newtonsoft.Json;

namespace DataProcessing.Test
{
    internal class UnitTestProcessor<TIn, TOut> : ProcessorBase, ISingleOutputProcessor where TIn: IId where TOut: IId
    {
        private readonly Func<TIn, TOut> processFunc;

        public UnitTestProcessor(string displayName, Func<TIn, TOut> processFunc)
            : base(displayName, new []{ typeof(TIn).Name })
        {
            this.processFunc = processFunc;
        }

        public string OutputTypeName { get; } = typeof(TOut).Name;
        private IList<DataModificationType> ModificationTypes { get; set; } = new[] {DataModificationType.Created, DataModificationType.Replaced};
        public bool PostponeProcessing { get; private set; }
        public DataReference MissingDataReference { get; private set; }
        public TimeSpan MaxPostponeTime { get; private set; }
        public int PostponeAttemptsCount { get; private set; }


        public override Task<IProcessorResult> Process(DataModificationType modificationType, string dataType, string inputId, string inputJson)
        {
            if (!ModificationTypes.Contains(modificationType))
                return Task.FromResult<IProcessorResult>(new NotInterestedProcessorResult());

            if (PostponeProcessing)
            {
                PostponedItemsCount++;
                return Task.FromResult<IProcessorResult>(
                    new PostponedProcessorResult(
                        new PostponedProcessingObject(
                            DisplayName,
                            modificationType,
                            dataType,
                            inputId,
                            MissingDataReference != null ? new List<DataReference> {MissingDataReference} : new List<DataReference>(),
                            MaxPostponeTime,
                            PostponeAttemptsCount,
                            DateTime.UtcNow)));
            }

            var unitTestInputObject = JsonConvert.DeserializeObject<TIn>(inputJson);
            var result = processFunc(unitTestInputObject);
            ProcessedItemsCount++;
            var processorResult = new SuccessProcessorResult(
                "",
                true,
                new List<SerializedObject>
                {
                    new SerializedObject(
                        result.Id,
                        OutputTypeName,
                        JsonConvert.SerializeObject(result))
                });
            return Task.FromResult<IProcessorResult>(processorResult);
        }

        public int ProcessedItemsCount { get; private set; }
        public void ResetProcessedItemsCount()
        {
            ProcessedItemsCount = 0;
        }

        public int PostponedItemsCount { get; private set; }
        public void ResetPostponedItemsCount()
        {
            PostponedItemsCount = 0;
        }

        public void ActivatePostponing(DataReference missingData = null, TimeSpan? maxPostponeTime = null, int attempts = 1)
        {
            PostponeProcessing = true;
            MissingDataReference = missingData;
            MaxPostponeTime = maxPostponeTime ?? TimeSpan.FromMinutes(5);
            PostponeAttemptsCount = attempts;
        }

        public void DeativatePostponing()
        {
            PostponeProcessing = false;
        }
    }
}