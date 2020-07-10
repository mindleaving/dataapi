using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Objects;
using Newtonsoft.Json;

namespace DataProcessing.ProcessingPostponing
{
    /// <summary>
    /// NOTE: This processor works in tandem with <see cref="PostponedProcessingRunner"/>.
    /// This processor is responsible for listening for new data and subscribing/unsubscribing to the relevant data types
    /// <see cref="PostponedProcessingRunner"/> is responsible for executing processors for which all conditions are met (no missing data) and remove the postponed processing object.
    /// </summary>
    public class PostponedProcessingObjectUpdateProcessor : ProcessorBase
    {
        private readonly IDataApiClient dataApiClient;
        private readonly ConcurrentDictionary<string, PostponedProcessingObject> postponedObjects = new ConcurrentDictionary<string, PostponedProcessingObject>();

        public PostponedProcessingObjectUpdateProcessor(IDataApiClient dataApiClient)
            : base(nameof(PostponedProcessingObjectUpdateProcessor), DataApiClient.GetCollectionName<PostponedProcessingObject>())
        {
            this.dataApiClient = dataApiClient;
        }

        public override void Initialize()
        {
            base.Initialize();
            var postponedProcessingObjects = Task.Run(async () => await dataApiClient.GetManyAsync<PostponedProcessingObject>()).Result;
            foreach (var obj in postponedProcessingObjects)
            {
                postponedObjects.TryAdd(obj.Id, obj);
                RegisterInputType(obj.DataType);
            }
        }

        public override Task<IProcessorResult> Process(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputObjectJson)
        {
            if(dataType == DataApiClient.GetCollectionName<PostponedProcessingObject>())
            {
                UpdatePostponedObjects(modificationType, inputId, inputObjectJson);
                return Task.FromResult<IProcessorResult>(new SuccessProcessorResult("Postponed objects updated", true));
            }

            var matchingObjects = postponedObjects.Values
                .Where(obj => obj.MissingData.Any(dataReference => dataReference.DataType == dataType && dataReference.Id == inputId))
                .ToList();

            if (!matchingObjects.Any())
                return Task.FromResult<IProcessorResult>(new NotInterestedProcessorResult());

            foreach (var obj in matchingObjects)
            {
                obj.MissingData.RemoveAll(dataReference => dataReference.DataType == dataType && dataReference.Id == inputId);
            }
            UnregisterDataTypeIfNoLongerNeeded(dataType);

            var summary = $"{matchingObjects.Count} {nameof(PostponedProcessingObject)} were updated";
            var outputObjects = matchingObjects
                .Select(obj => new SerializedObject(obj.Id, obj.GetType().Name, JsonConvert.SerializeObject(obj)))
                .ToList();
            return Task.FromResult<IProcessorResult>(new SuccessProcessorResult(summary, true, outputObjects));
        }

        private void UnregisterUnnecessaryInputTypes()
        {
            var necessaryInputTypes = postponedObjects.Values.SelectMany(x => x.MissingData).Select(x => x.DataType)
                .Concat(new[] {DataApiClient.GetCollectionName<PostponedProcessingObject>()})
                .Distinct();
            var unnecessaryInputTypes = InputTypes.Except(necessaryInputTypes);
            foreach (var unnecessaryInputType in unnecessaryInputTypes)
            {
                UnregisterInputType(unnecessaryInputType);
            }
        }

        private void UnregisterDataTypeIfNoLongerNeeded(string dataType)
        {
            if (!postponedObjects.Values.SelectMany(x => x.MissingData).Select(x => x.DataType).Distinct().Contains(dataType))
                UnregisterInputType(dataType);
        }

        private void UpdatePostponedObjects(DataModificationType modificationType, string inputId, string inputObjectJson)
        {
            switch (modificationType)
            {
                case DataModificationType.Created:
                case DataModificationType.Replaced:
                    var postponedObject = JsonConvert.DeserializeObject<PostponedProcessingObject>(inputObjectJson);
                    postponedObjects.AddOrUpdate(inputId, postponedObject, (s, obj) => postponedObject);
                    foreach (var dataType in postponedObject.MissingData.Select(x => x.DataType))
                    {
                        RegisterInputType(dataType);
                    }
                    break;
                case DataModificationType.Deleted:
                    postponedObjects.TryRemove(inputId, out _);
                    UnregisterUnnecessaryInputTypes();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null);
            }
        }
    }
}
