using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Objects;

namespace DataProcessing
{
    public interface IProcessor
    {
        void Initialize();
        string DisplayName { get; }
        IReadOnlyCollection<string> InputTypes { get; }

        Task<IProcessorResult> Process(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputObjectJson);

        event EventHandler<InputTypeRegistrationEventArgs> InputTypeAdded;
        event EventHandler<InputTypeRegistrationEventArgs> InputTypeRemoved;
    }

    public interface ISingleOutputProcessor : IProcessor
    {
        string OutputTypeName { get; }
    }


    public interface ITask
    {
        string DisplayName { get; }
        Task<ExecutionResult> Action(CancellationToken cancellationToken);
    }
    public interface IPeriodicTask : ITask
    {
        TimeSpan Period { get; }
    }

    public interface IDailyTask : ITask
    {
        TimeSpan TimeOfDayUtc { get; }
    }
}
