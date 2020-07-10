using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataProcessing
{
    public class TaskLoadingTask : IPeriodicTask
    {
        private readonly TaskLoader taskLoader;
        private readonly TaskDatabase taskDatabase;

        public TaskLoadingTask(
            TaskLoader taskLoader, 
            TaskDatabase taskDatabase)
        {
            this.taskLoader = taskLoader;
            this.taskDatabase = taskDatabase;
        }

        public string DisplayName { get; } = nameof(TaskLoadingTask);
        public TimeSpan Period { get; } = TimeSpan.FromMinutes(30);

        public Task<ExecutionResult> Action(CancellationToken cancellationToken)
        {
            var loadedTasks = taskLoader.Load();
            var addedTasks = new List<ITask>();
            foreach (var loadedTask in loadedTasks)
            {
                var isNew = !taskDatabase.Tasks.Contains(loadedTask);
                if(!isNew)
                    continue;
                taskDatabase.Add(loadedTask);
                addedTasks.Add(loadedTask);
            }
            if(!addedTasks.Any())
                return Task.FromResult(new ExecutionResult(true, false, "No new tasks found"));

            var aggregatedTaskDisplayNames = string.Join(", ", addedTasks.Select(task => task.DisplayName));
            var summary = $"Added tasks {aggregatedTaskDisplayNames}";
            return Task.FromResult(new ExecutionResult(true, true, summary));
        }
    }
}