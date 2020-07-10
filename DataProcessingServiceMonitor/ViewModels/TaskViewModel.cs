using System;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using DataProcessing.Logging;
using SharedViewModels.Repositories;
using SharedViewModels.ViewModels;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class TaskViewModel : NotifyPropertyChangedBase, IExecutorViewModel
    {
        public TaskViewModel(TaskDetails taskDetails)
        {
            Details = taskDetails;
        }

        public IExecutorDetails Details { get; }
        private TaskDetails TaskDetails => (TaskDetails) Details;
        public string WindowTitle => $"{Details.ExecutorType} {Details.Name}";
        public ConcurrentObservableCollection<LogEntryViewModel> LogEntries { get; } = new ConcurrentObservableCollection<LogEntryViewModel>();

        private DateTime? lastExecutionTime;
        public DateTime? LastExecutionTime
        {
            get => lastExecutionTime;
            set
            {
                if(value < lastExecutionTime)
                    return;
                lastExecutionTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NextExecutionTime));
            }
        }
        public DateTime? NextExecutionTime => LastExecutionTime?.Add(TaskDetails.Period);

        public void EstimatePeriod()
        {
            var executionStartedLogEntries = LogEntries
                .Where(logEntry => logEntry.DetailsType == nameof(ExecutionStartLogEntryDetails))
                .OrderBy(logEntry => logEntry.Timestamp)
                .ToList();
            if(executionStartedLogEntries.Count < 2)
                return;
            var medianTimeDifferenceInSeconds = executionStartedLogEntries
                .Zip(executionStartedLogEntries.Skip(1), (l1, l2) => l2.Timestamp - l1.Timestamp)
                .Select(timeSpan => timeSpan.TotalSeconds)
                .Median();
            TaskDetails.Period = TimeSpan.FromSeconds(medianTimeDifferenceInSeconds);
            OnPropertyChanged(nameof(NextExecutionTime));
        }

        public void UpadateExecutionTime()
        {
            var cutoffTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            TaskDetails.ExecutionTimeLast24Hours = LogEntries
                .Where(logEntry => logEntry.ExecutionTime != null)
                .Where(logEntry => logEntry.Timestamp > cutoffTime)
                .Select(logEntry => logEntry.ExecutionTime)
                .Sum(Unit.Second);
        }
    }
}
