using System;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using SharedViewModels.Repositories;
using SharedViewModels.ViewModels;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class ProcessorViewModel : NotifyPropertyChangedBase, IExecutorViewModel
    {
        public ProcessorViewModel(ProcessorDetails processorDetails)
        {
            Details = processorDetails;
        }

        public string WindowTitle => $"{Details.ExecutorType} {Details.Name}";

        public IExecutorDetails Details { get; }
        public ProcessorDetails ProcessorDetails => (ProcessorDetails) Details;
        public ConcurrentObservableCollection<LogEntryViewModel> LogEntries { get; } = new ConcurrentObservableCollection<LogEntryViewModel>();

        public void UpadateExecutionTime()
        {
            var cutoffTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            ProcessorDetails.ExecutionTimeLast24Hours = LogEntries
                .Where(logEntry => logEntry.ExecutionTime != null)
                .Where(logEntry => logEntry.Timestamp > cutoffTime)
                .Select(logEntry => logEntry.ExecutionTime)
                .Sum(Unit.Second);
        }
    }
}
