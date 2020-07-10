using System;
using Commons.Physics;
using DataProcessing;

namespace DataProcessingServiceMonitor.ViewModels
{
    public class TaskDetails : IExecutorDetails
    {
        public TaskDetails(string name)
        {
            Name = name;
            Period = TimeSpan.Zero;
        }

        public string Name { get; }
        public DataProcessingServiceExecutorType ExecutorType { get; } = DataProcessingServiceExecutorType.Task;
        public TimeSpan Period { get; set; }
        public string PeriodString => $"{Period.Days}days {Period.Hours}h {Period.Minutes}min {Period.Seconds}s";
        public UnitValue ExecutionTimeLast24Hours { get; set; }
    }
}