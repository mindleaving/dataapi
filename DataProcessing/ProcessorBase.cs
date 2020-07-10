using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Objects;

namespace DataProcessing
{
    public abstract class ProcessorBase : IProcessor
    {
        protected readonly object inputTypeLock = new object();

        protected ProcessorBase(string displayName, IReadOnlyCollection<string> inputTypes)
        {
            DisplayName = displayName;
            this.inputTypes = new List<string>(inputTypes);
        }

        protected ProcessorBase(string displayName, string inputType)
            : this(displayName, new []{ inputType })
        {
        }

        protected void RegisterInputType(string dataType)
        {
            lock (inputTypeLock)
            {
                if(inputTypes.Contains(dataType))
                    return;
                inputTypes.Add(dataType);
            }
            InputTypeAdded?.Invoke(this, new InputTypeRegistrationEventArgs(dataType));
        }

        protected void UnregisterInputType(string dataType)
        {
            lock (inputTypeLock)
            {
                if(!inputTypes.Remove(dataType))
                    return;
            }
            InputTypeRemoved?.Invoke(this, new InputTypeRegistrationEventArgs(dataType));
        }

        public virtual void Initialize()
        {
            foreach (var inputType in InputTypes)
            {
                RegisterInputType(inputType);
            }
        }

        public string DisplayName { get; }
        private readonly List<string> inputTypes;
        public IReadOnlyCollection<string> InputTypes => inputTypes;

        public abstract Task<IProcessorResult> Process(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputObjectJson);

        public event EventHandler<InputTypeRegistrationEventArgs> InputTypeAdded;
        public event EventHandler<InputTypeRegistrationEventArgs> InputTypeRemoved;
    }
}
