using System;

namespace DataProcessing
{
    public class ScheduledTask
    {
        public ScheduledTask(DateTime scheduledExecutionTime, ITask task)
        {
            ScheduledExecutionTime = scheduledExecutionTime;
            Task = task;
        }

        public DateTime ScheduledExecutionTime { get; }
        public ITask Task { get; }
    }
}