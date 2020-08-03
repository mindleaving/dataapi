using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Commons.Extensions;
using DataAPI.Client;
using DataProcessing;
using DataProcessing.Logging;
using DataProcessingServiceMonitor.Views;
using SharedViewModels.Repositories;
using SharedViewModels.ViewModels;
using SharedWpfControls.Helpers;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly LogEntryMonitor logEntryMonitor;
        private readonly IClosable mainWindow;
        private readonly Timer periodUpdateTimer;
        private readonly Timer executionTimeUpdateTimer;

        public MainViewModel(
            IDataApiClient dataApiClient,
            LogEntryMonitor logEntryMonitor, 
            IClosable mainWindow)
        {
            this.logEntryMonitor = logEntryMonitor;
            this.mainWindow = mainWindow;
            logEntryMonitor.NewLogEntry += LogEntryMonitor_NewLogEntry;
            logEntryMonitor.MonitoringStateChanged += LogEntryMonitor_MonitoringStateChanged;
            logEntryMonitor.HistoricLogEntriesLoaded += LogEntryMonitor_HistoricLogEntriesLoaded;

            UserSwitchViewModel = new UserSwitchViewModelFactory(
                dataApiClient,
                LoginSuccessful,
                LoginFailed).Create();
            OpenProcessorWindowCommand = new RelayCommand(OpenProcessorWindow, () => SelectedProcessor != null);
            OpenTaskWindowCommand = new RelayCommand(OpenTaskWindow, () => SelectedTask != null);
            UpdateTaskPeriodsCommand = new RelayCommand(UpdateTaskPeriod);
            UpdateExecutionTimesCommand = new RelayCommand(UpdateExecutionTimes);

            periodUpdateTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            periodUpdateTimer.Elapsed += (sender, args) => UpdateTaskPeriod();
            executionTimeUpdateTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            executionTimeUpdateTimer.Elapsed += (sender, args) => UpdateExecutionTimes();
        }

        private void LogEntryMonitor_MonitoringStateChanged(object sender, bool isMonitoring)
        {
            IsMonitoring = isMonitoring;
        }

        private async void LogEntryMonitor_HistoricLogEntriesLoaded(object sender, EventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(3)); // HACK. Wait necessary because not all log entries are processed when this event is fired.
            UpdateTaskPeriod();
            periodUpdateTimer.Start();
            UpdateExecutionTimes();
            executionTimeUpdateTimer.Start();
        }

        private void LogEntryMonitor_NewLogEntry(object sender, DataProcessingServiceLog logEntry)
        {
            Application.Current.Dispatcher.BeginInvoke((Action) (() =>
            {
                var logEntryViewModel = new LogEntryViewModel(logEntry);
                if(IsAppropriateForAllMessagesList(logEntry))
                {
                    AllLogMessages.Add(logEntryViewModel);
                }
                if (logEntryViewModel.IsError)
                    ErrorLogMessages.Add(logEntryViewModel);
                if (logEntryViewModel.ExecutorType.HasValue)
                {
                    switch (logEntryViewModel.ExecutorType.Value)
                    {
                        case DataProcessingServiceExecutorType.Processor:
                            if (logEntryViewModel.ProcessorName != "Unknown")
                            {
                                var matchingProcessor = Processors.SingleOrDefault(p => p.Details.Name == logEntryViewModel.ProcessorName);
                                if (matchingProcessor == null)
                                {
                                    matchingProcessor = new ProcessorViewModel(
                                        new ProcessorDetails(
                                            logEntryViewModel.ProcessorName,
                                            logEntryViewModel.InputTypeName, 
                                            logEntryViewModel.OutputTypeName)
                                    );
                                    // Sort alphabetically
                                    var insertIndex = Processors.Count(processor => 
                                        processor.Details.Name.CompareTo(matchingProcessor.Details.Name) < 0);
                                    Processors.Insert(insertIndex, matchingProcessor);
                                }

                                matchingProcessor.LogEntries.Add(logEntryViewModel);
                            }
                            break;
                        case DataProcessingServiceExecutorType.Task:
                            var matchingTask = Tasks.SingleOrDefault(t => t.Details.Name == logEntryViewModel.ProcessorName);
                            if (matchingTask == null)
                            {
                                matchingTask = new TaskViewModel(new TaskDetails(logEntryViewModel.ProcessorName));
                                // Sort alphabetically
                                var insertIndex = Tasks.Count(task => task.Details.Name.CompareTo(matchingTask.Details.Name) <= 0);
                                Tasks.Insert(insertIndex, matchingTask);
                            }
                            matchingTask.LogEntries.Add(logEntryViewModel);
                            if (logEntry.Details is ExecutionSummaryLogEntryDetails 
                                && (!matchingTask.LastExecutionTime.HasValue || matchingTask.LastExecutionTime < logEntry.Timestamp))
                            {
                                matchingTask.LastExecutionTime = logEntry.Timestamp;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }));
        }

        private bool IsAppropriateForAllMessagesList(DataProcessingServiceLog logEntry)
        {
            if (logEntry.Details is ExecutionStartLogEntryDetails)
                return false;
            if (logEntry.Details is ExecutionSummaryLogEntryDetails executionSummaryLogEntryDetails)
            {
                if (executionSummaryLogEntryDetails.IsError)
                    return true;
                if (!executionSummaryLogEntryDetails.IsWorkDone)
                    return false;
            }
            return true;
        }

        public ConcurrentObservableCollection<ProcessorViewModel> Processors { get; } = new ConcurrentObservableCollection<ProcessorViewModel>();
        public ConcurrentObservableCollection<TaskViewModel> Tasks { get; } = new ConcurrentObservableCollection<TaskViewModel>();

        public ConcurrentObservableCollection<LogEntryViewModel> AllLogMessages { get; } = new ConcurrentObservableCollection<LogEntryViewModel>();
        public ConcurrentObservableCollection<LogEntryViewModel> ErrorLogMessages { get; } = new ConcurrentObservableCollection<LogEntryViewModel>();

        private ProcessorViewModel selectedProcessor;
        public ProcessorViewModel SelectedProcessor
        {
            get => selectedProcessor;
            set
            {
                selectedProcessor = value;
                OnPropertyChanged();
            }
        }

        private TaskViewModel selectedTask;
        public TaskViewModel SelectedTask
        {
            get => selectedTask;
            set
            {
                selectedTask = value;
                OnPropertyChanged();
            }
        }

        public string MonitoringState => IsMonitoring ? "Active" : "Not started/crashed";

        private bool isMonitoring;
        public bool IsMonitoring
        {
            get => isMonitoring;
            private set
            {
                isMonitoring = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonitoringState));
            }
        }

        public string ServiceState => IsServiceRunning ? "Active" : "Not started/crashed";
        public bool IsServiceRunning { get; private set; } = true;

        public UserSwitchViewModel UserSwitchViewModel { get; }
        public ICommand OpenProcessorWindowCommand { get; }
        public ICommand OpenTaskWindowCommand { get; }
        public ICommand UpdateTaskPeriodsCommand { get; }
        public ICommand UpdateExecutionTimesCommand { get; }

        public void LoginSuccessful()
        {
            logEntryMonitor.StartMonitoring();
        }

        private void LoginFailed()
        {
            mainWindow.Close(false);
        }

        private void OpenProcessorWindow()
        {
            if(SelectedProcessor == null)
                return;
            var processorWindow = new ProcessorTaskWindow
            {
                ViewModel = SelectedProcessor
            };
            processorWindow.Show();
        }

        private void OpenTaskWindow()
        {
            if(SelectedTask == null)
                return;
            var taskWindow = new ProcessorTaskWindow
            {
                ViewModel = SelectedTask
            };
            taskWindow.Show();
        }

        private void UpdateTaskPeriod()
        {
            Tasks.ForEach(task => task.EstimatePeriod());
        }

        private void UpdateExecutionTimes()
        {
            Processors.ForEach(processor => processor.UpadateExecutionTime());
            Tasks.ForEach(task => task.UpadateExecutionTime());
        }
    }
}
