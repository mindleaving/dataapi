using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Physics;
using DataAPI.Client;
using DataProcessing.Logging;

namespace DataProcessing
{
    public class PeriodicTasksRunner
    {
        private readonly IDataProcessingServiceLogger dataProcessingServiceLogger;
        private CancellationTokenSource cancellationTokenSource;
        private Task runnerTask;
        private readonly IDataApiClient dataApiClient;
        private readonly TaskDatabase taskDatabase;
        private readonly List<ScheduledTask> taskQueue;

        public PeriodicTasksRunner(
            IDataApiClient dataApiClient,
            TaskDatabase taskDatabase,
            IDataProcessingServiceLogger dataProcessingServiceLogger)
        {
            this.dataApiClient = dataApiClient;
            this.taskDatabase = taskDatabase;

            this.dataProcessingServiceLogger = dataProcessingServiceLogger;
            taskQueue = InitializeTaskQueue();
        }

        public bool IsRunning { get; private set; }

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
            cancellationTokenSource?.Cancel();
            try
            {
                runnerTask?.Wait();
            }
            finally
            {
                IsRunning = false;
            }
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog($"{nameof(PeriodicTasksRunner)} started", null));
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog(
                        $"{nameof(PeriodicTasksRunner)} running for " +
                        $"'{taskDatabase.Tasks.Select(task => task.DisplayName).Aggregate((a, b) => a + ", " + b)}'",
                        null));
            }
            catch
            {
                // Ignore logging errors
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!dataApiClient.IsAvailable())
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(60));
                    continue;
                }
                if (!dataApiClient.IsLoggedIn && !dataApiClient.RetryLogin().IsAuthenticated)
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(60));
                    continue;
                }
                var nextTask = TakeNextTask(taskQueue);
                var waitTime = nextTask != null
                    ? nextTask.ScheduledExecutionTime - DateTime.UtcNow
                    : TimeSpan.FromSeconds(30);
                if (waitTime > TimeSpan.Zero)
                {
                    cancellationToken.WaitHandle.WaitOne(waitTime);
                }
                if (nextTask == null)
                    continue;
                try
                {
                    await LogTaskStarting(nextTask);
                }
                catch
                {
                    // Ignore. Failing log shouldn't affect task execution
                }
                var stopWatch = Stopwatch.StartNew();
                bool isSuccess;
                bool isWorkDone = false;
                string summary;
                try
                {
                    var executionResult = nextTask.Task.Action(cancellationToken).Result;
                    isSuccess = executionResult.IsSuccess;
                    isWorkDone = executionResult.IsWorkDone;
                    summary = executionResult.Summary;
                }
                catch (Exception e)
                {
                    isSuccess = false;
                    summary = e.InnermostException().Message;
                }
                stopWatch.Stop();
                try
                {
                    await LogTaskCompleted(nextTask, summary, isSuccess, isWorkDone, stopWatch);
                }
                catch
                {
                    // Ignore. Failing log shouldn't affect task execution
                }
                switch (nextTask.Task)
                {
                    case IPeriodicTask periodicTask:
                        taskQueue.Add(new ScheduledTask(nextTask.ScheduledExecutionTime.Add(periodicTask.Period), periodicTask));
                        break;
                    case IDailyTask dailyTask:
                        var tomorrowSameTime = nextTask.ScheduledExecutionTime.AddDays(1);
                        taskQueue.Add(new ScheduledTask(tomorrowSameTime, dailyTask));
                        break;
                }
            }

            try
            {
                await dataProcessingServiceLogger.Log(new DataProcessingServiceLog($"{nameof(PeriodicTasksRunner)} stopped", null));
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static ScheduledTask TakeNextTask(ICollection<ScheduledTask> taskQueue)
        {
            var nextTask = taskQueue.Any() ? taskQueue.MinimumItem(task => task.ScheduledExecutionTime.Ticks) : null;
            if (nextTask != null)
                taskQueue.Remove(nextTask);
            return nextTask;
        }

        private List<ScheduledTask> InitializeTaskQueue()
        {
            var scheduledTasks = new List<ScheduledTask>();
            foreach (var task in taskDatabase.Tasks)
            {
                ScheduledTask scheduledTask;
                switch (task)
                {
                    case IDailyTask dailyTask:
                        var executionTime = DateTime.UtcNow.Date.Add(dailyTask.TimeOfDayUtc);
                        if (executionTime < DateTime.UtcNow)
                            executionTime += TimeSpan.FromDays(1);
                        scheduledTask = new ScheduledTask(executionTime, task);
                        break;
                    case IPeriodicTask periodicTask:
                        scheduledTask = new ScheduledTask(DateTime.UtcNow, task);
                        break;
                    default:
                        scheduledTask = new ScheduledTask(DateTime.UtcNow, task);
                        break;
                }
                scheduledTasks.Add(scheduledTask);
            }
            return scheduledTasks;
        }

        private async Task LogTaskStarting(ScheduledTask task)
        {
            var logEntry = new DataProcessingServiceLog(
                $"Starting execution of task '{task.Task.DisplayName}'",
                new ExecutionStartLogEntryDetails(
                    DataProcessingServiceExecutorType.Task,
                    task.Task.DisplayName));
            await dataProcessingServiceLogger.Log(logEntry);
        }

        private async Task LogTaskCompleted(ScheduledTask task, string summary, bool isSuccess, bool isWorkDone, Stopwatch stopWatch)
        {
            var logEntry = new DataProcessingServiceLog(
                $"Execution of task '{task.Task.DisplayName}' finished: {summary}",
                new ExecutionSummaryLogEntryDetails(
                    DataProcessingServiceExecutorType.Task,
                    task.Task.DisplayName,
                    stopWatch.Elapsed.TotalSeconds.To(Unit.Second),
                    !isSuccess,
                    isWorkDone));
            await dataProcessingServiceLogger.Log(logEntry);
        }
    }
}
