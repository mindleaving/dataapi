using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Models;
using DataProcessing.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataProcessing.DataServices
{
    public class DataServiceProcessor : ProcessorBase, IDisposable
    {
        private readonly IDataApiClient dataApiClient;
        private readonly List<DataServiceDefinition> dataServiceDefinitions;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly Dictionary<string, Task> initTasks = new Dictionary<string, Task>();
        private readonly ConcurrentDictionary<string, IDataService> dataServices = new ConcurrentDictionary<string, IDataService>();

        public DataServiceProcessor(IDataApiClient dataApiClient)
            : base(nameof(DataServiceProcessor), DataApiClient.GetCollectionName<DataServiceDefinition>())
        {
            this.dataApiClient = dataApiClient;
            dataServiceDefinitions = LoadDataServiceDefinitions();
            var dataServiceDefinitionsCopy = dataServiceDefinitions.ToList();
            foreach (var dataServiceDefinition in dataServiceDefinitionsCopy)
            {
                var initTask = TransferExistingDataIfNewTarget(dataServiceDefinition, cancellationTokenSource.Token);
                initTasks.Add(dataServiceDefinition.Id, initTask);
            }
        }

        public override async Task<IProcessorResult> Process(DataModificationType modificationType, string dataType, string inputId, string inputObjectJson)
        {
            var initTaskSummaries = initTasks.Select(FormatInitTaskStatus).ToList();
            var finishedInitTasks = initTasks.Where(kvp => kvp.Value.Status.InSet(TaskStatus.RanToCompletion, TaskStatus.Canceled, TaskStatus.Faulted)).ToList();
            finishedInitTasks.Select(kvp => kvp.Key).ForEach(id => initTasks.Remove(id));
            if (dataType == DataApiClient.GetCollectionName<DataServiceDefinition>())
            {
                var dataServiceDefinitionId = inputId;
                UpdateDataServiceDefinitions(modificationType, inputId, inputObjectJson);
                if (modificationType != DataModificationType.Deleted)
                {
                    var dataServiceDefinition = JsonConvert.DeserializeObject<DataServiceDefinition>(inputObjectJson);
#pragma warning disable 4014
                    var initTask = TransferExistingDataIfNewTarget(dataServiceDefinition, cancellationTokenSource.Token);
                    initTasks.Add(dataServiceDefinition.Id, initTask);
#pragma warning restore 4014
                    bool initTaskCompleted;
                    try
                    {
                        initTaskCompleted = initTask.Wait(TimeSpan.FromSeconds(5));
                    }
                    catch
                    {
                        initTaskCompleted = true;
                    }
                    if (initTaskCompleted)
                    {
                        if (initTask.Status == TaskStatus.RanToCompletion)
                            initTaskSummaries.Add($"Init task for service '{dataServiceDefinitionId}' finished without errors. ");
                        else if (initTask.Status == TaskStatus.Faulted)
                            initTaskSummaries.Add($"Init task for service '{dataServiceDefinitionId}' failed: " + initTask.Exception.InnermostException().Message + ". ");
                        else if (initTask.Status == TaskStatus.Canceled)
                            initTaskSummaries.Add($"Init task for service '{dataServiceDefinitionId}' was cancelled. ");
                        initTasks.Remove(dataServiceDefinition.Id);
                    }
                    else
                    {
                        initTaskSummaries.Add($"Init task for service '{dataServiceDefinitionId}' is running. ");
                    }
                }

                var summary = $"Data service definitions '{dataServiceDefinitionId}' {modificationType.ToString().ToLowerInvariant()}";
                if (initTaskSummaries.Any())
                    summary += "\n" + string.Join("\n", initTaskSummaries);
                return new SuccessProcessorResult(summary, true);
            }

            var matchingDataServiceDefinitions = dataServiceDefinitions.Where(x => x.DataType == dataType).ToList();
            if(!matchingDataServiceDefinitions.Any())
                return new NotInterestedProcessorResult();

            var failingServices = new List<Tuple<DataServiceDefinition,string>>();
            foreach (var dataServiceDefinition in matchingDataServiceDefinitions)
            {
                try
                {
                    var dataService = await CreateDataServiceAsync(dataServiceDefinition);
                    if (modificationType == DataModificationType.Deleted)
                    {
                        await dataService.DeleteAsync(inputId);
                    }
                    else
                    {
                        var jObject = JObject.Parse(inputObjectJson);
                        await dataService.TransferAsync(jObject, dataServiceDefinition.Fields, inputId);
                    }
                }
                catch (Exception e)
                {
                    failingServices.Add(new Tuple<DataServiceDefinition, string>(dataServiceDefinition, e.InnermostException().Message));
                }
            }

            if (failingServices.Any())
            {
                var joinedFailedTargets = string.Join("\n", failingServices.Select(tuple => tuple.Item1.Target + ": " + tuple.Item2));
                var summary = modificationType == DataModificationType.Deleted
                    ? $"Could not remove {dataType} with ID '{inputId}' from {failingServices.Count} targets:\n{joinedFailedTargets}\n"
                    : $"Could not transfer {dataType} with ID '{inputId}' to {failingServices.Count} targets:\n{joinedFailedTargets}\n";
                if (matchingDataServiceDefinitions.Count > failingServices.Count)
                {
                    var joinedTargets = string.Join("\n", matchingDataServiceDefinitions.Except(failingServices.Select(x => x.Item1)).Select(x => x.Target.ToString()));
                    summary += modificationType == DataModificationType.Deleted
                        ? $"Successfully removed from {matchingDataServiceDefinitions.Count} targets:\n{joinedTargets}"
                        : $"Successfully transferred to {matchingDataServiceDefinitions.Count} targets:\n{joinedTargets}";
                }
                if (initTaskSummaries.Any())
                    summary += "\n" + string.Join("\n", initTaskSummaries);
                return new ErrorProcessorResult(summary);
            }
            else
            {
                var joinedTargets = string.Join("\n", matchingDataServiceDefinitions.Select(x => x.Target.ToString()));
                var summary = modificationType == DataModificationType.Deleted
                    ? $"Removed {dataType} with ID '{inputId}' from {matchingDataServiceDefinitions.Count} targets:\n{joinedTargets}"
                    : $"Transferred {dataType} with ID '{inputId}' to {matchingDataServiceDefinitions.Count} targets:\n{joinedTargets}";
                if (initTaskSummaries.Any())
                    summary += "\n" + string.Join("\n", initTaskSummaries);
                return new SuccessProcessorResult(summary, true);
            }
        }

        private static string FormatInitTaskStatus(KeyValuePair<string, Task> kvp)
        {
            var dataServiceDefinitionId = kvp.Key;
            var initTask = kvp.Value;
            if (initTask.Status == TaskStatus.RanToCompletion)
                return $"Init task for service '{dataServiceDefinitionId}' finished without errors. ";
            else if (initTask.Status == TaskStatus.Faulted)
                return $"Init task for service '{dataServiceDefinitionId}' failed: " + initTask.Exception.InnermostException().Message + ". ";
            else if (initTask.Status == TaskStatus.Canceled)
                return $"Init task for service '{dataServiceDefinitionId}' was cancelled. ";
            else
                return $"Init task for service '{dataServiceDefinitionId}' is running";
        }

        private Task TransferExistingDataIfNewTarget(DataServiceDefinition dataServiceDefinition, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    var dataService = CreateDataServiceAsync(dataServiceDefinition).Result;
                    var idQuery = $"SELECT * FROM {dataServiceDefinition.DataType}";
                    if (string.IsNullOrWhiteSpace(dataServiceDefinition.Filter))
                        idQuery += $" WHERE {dataServiceDefinition.Filter}";
                    dataApiClient.SearchAsync(idQuery, ResultFormat.Json).Result.ForEachSearchResult(
                        jObject =>
                        {
                            var id = jObject.Value<string>("_id");
                            if (dataService.ObjectExistsAsync(id).Result)
                                return;
                            var data = (JObject) jObject["Data"];
                            dataService.TransferAsync(data, dataServiceDefinition.Fields, id).Wait();
                        }).Wait();
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private async Task<IDataService> CreateDataServiceAsync(DataServiceDefinition dataServiceDefinition)
        {
            if (dataServices.TryGetValue(dataServiceDefinition.Id, out var dataService))
                return dataService;
            switch (dataServiceDefinition.Target.Type)
            {
                case DataServiceTargetType.File:
                    dataService = new FileDataService((FileDataServiceTarget) dataServiceDefinition.Target);
                    break;
                case DataServiceTargetType.Sql:
                    dataService = new SqlDataService((SqlDataServiceTarget) dataServiceDefinition.Target);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await dataService.InitializeAsync();
            dataServices.TryAdd(dataServiceDefinition.Id, dataService);
            return dataService;
        }

        private void UpdateDataServiceDefinitions(DataModificationType modificationType, string inputId, string inputObjectJson)
        {
            if (modificationType == DataModificationType.Deleted)
            {
                dataServiceDefinitions.RemoveAll(x => x.Id == inputId);
                dataServices.TryRemove(inputId, out _);
            }
            else
            {
                var dataServiceDefinition = JsonConvert.DeserializeObject<DataServiceDefinition>(inputObjectJson);
                dataServiceDefinitions.RemoveAll(x => x.Id == inputId);
                dataServiceDefinitions.Add(dataServiceDefinition);
            }
        }

        private List<DataServiceDefinition> LoadDataServiceDefinitions()
        {
            return Task.Run(async () => await dataApiClient.GetManyAsync<DataServiceDefinition>()).Result.ToList();
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            Task.WaitAll(initTasks.Values.ToArray(), TimeSpan.FromSeconds(30));
            cancellationTokenSource.Dispose();
        }
    }
}
