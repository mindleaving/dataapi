using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;
using DataProcessing.DataServices;
using DataProcessing.Logging;
using DataProcessing.ProcessingPostponing;

namespace DataProcessing
{
    public class DataProcessingServiceSetup
    {
        public DataProcessingServiceSetup(
            IDataApiClient dataApiClient,
            LoginInformation apiLoginInformation,
            DataProcessingServiceSettings settings)
        {
            var authenticationResult = dataApiClient.Login(apiLoginInformation.Username, apiLoginInformation.Password);
            if(authenticationResult == null)
                throw new Exception($"DataAPI-authentication result was null for user '{apiLoginInformation.Username}'");
            if (!authenticationResult.IsAuthenticated)
                throw new Exception("Could not log into Data API");
            var dataProcessingServiceLogger = new DataProcessingServiceLogger(dataApiClient);

            try
            {
                var processorDefinitionDirectory = settings.ProcessorDefinitionDirectory;
                var processorLoader = new ProcessorLoader(processorDefinitionDirectory);
                Processors = new List<IProcessor>
                {
                    new PostponedProcessingObjectUpdateProcessor(dataApiClient),
                    new DataServiceProcessor(dataApiClient)
                }.Concat(processorLoader.Load()).ToList();
                PostponedProcessingRunner = new PostponedProcessingRunner(
                    dataApiClient,
                    Processors,
                    dataProcessingServiceLogger);
                var processorDatabase = new ProcessorDatabase(Processors.Concat(new []{ PostponedProcessingRunner }));
                Distributor = new Distributor(
                    dataApiClient,
                    processorDatabase, 
                    dataProcessingServiceLogger);


                var taskDefinitionDirectory = settings.TaskDefinitionDirectory;
                var taskLoader = new TaskLoader(taskDefinitionDirectory);
                var taskDatabase = new TaskDatabase();
                Tasks = new List<ITask>
                {
                    new LogTruncationTask(dataApiClient, TimeSpan.FromDays(3)),
                    new TaskLoadingTask(taskLoader, taskDatabase),
                    new ProcessorLoadingTask(processorLoader, processorDatabase),
                }.Concat(taskLoader.Load()).ToList();
                Tasks.ForEach(taskDatabase.Add);
                PeriodicTasksRunner = new PeriodicTasksRunner(
                    dataApiClient, 
                    taskDatabase,
                    dataProcessingServiceLogger);
            }
            catch (Exception e)
            {
                dataProcessingServiceLogger.Log(
                        new DataProcessingServiceLog(
                            $"Startup of DataProcessing service failed: {e}",
                            new CrashLogEntryDetails("DataProcessingService", e.InnermostException().Message)))
                    .Wait();
                throw;
            }
        }

        private IList<string> ParseCommaSeparatedValues(string csv)
        {
            return csv
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }


        public List<IProcessor> Processors { get; }
        public List<ITask> Tasks { get; }
        public Distributor Distributor { get; }
        public PostponedProcessingRunner PostponedProcessingRunner { get; }
        public PeriodicTasksRunner PeriodicTasksRunner { get; }
    }
}
