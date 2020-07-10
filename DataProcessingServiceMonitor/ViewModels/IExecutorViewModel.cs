using DataProcessing;
using SharedViewModels.Repositories;

namespace DataProcessingServiceMonitor.ViewModels
{
    public interface IExecutorViewModel
    {
        IExecutorDetails Details { get; }
        ConcurrentObservableCollection<LogEntryViewModel> LogEntries { get; }
        string WindowTitle { get; }
    }

    public interface IExecutorDetails
    {
        string Name { get; }
        DataProcessingServiceExecutorType ExecutorType { get; }
    }
}