using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataProcessing
{
    public class ProcessorLoadingTask : IPeriodicTask
    {
        private readonly ProcessorLoader processorLoader;
        private readonly ProcessorDatabase processorDatabase;

        public ProcessorLoadingTask(
            ProcessorLoader processorLoader,
            ProcessorDatabase processorDatabase)
        {
            this.processorLoader = processorLoader;
            this.processorDatabase = processorDatabase;
        }

        public string DisplayName { get; } = nameof(ProcessorLoadingTask);
        public TimeSpan Period { get; } = TimeSpan.FromMinutes(30);

        public Task<ExecutionResult> Action(CancellationToken cancellationToken)
        {
            var loadedProcessors = processorLoader.Load();
            var addedProcessors = new List<IProcessor>();
            foreach (var loadedProcessor in loadedProcessors)
            {
                var isNew = processorDatabase.GetAll().All(processor => processor.DisplayName != loadedProcessor.DisplayName);
                if(!isNew)
                    continue;
                processorDatabase.Add(loadedProcessor);
                addedProcessors.Add(loadedProcessor);
            }
            if(!addedProcessors.Any())
                return Task.FromResult(new ExecutionResult(true, false, "No new processors found"));

            var aggregatedProcessorDisplayNames = string.Join(", ", addedProcessors.Select(processor => processor.DisplayName));
            var summary = $"Added processors {aggregatedProcessorDisplayNames}";
            return Task.FromResult(new ExecutionResult(true, true, summary));
        }
    }
}