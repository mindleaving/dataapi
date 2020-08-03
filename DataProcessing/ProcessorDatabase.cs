using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;

namespace DataProcessing
{
    public class ProcessorDatabase
    {
        private readonly ConcurrentDictionary<string, List<IProcessor>> typeMap = new ConcurrentDictionary<string, List<IProcessor>>();
        private readonly ConcurrentBag<IProcessor> processors = new ConcurrentBag<IProcessor>();
        private readonly object typeMapListLock = new object();

        public ProcessorDatabase(IEnumerable<IProcessor> processors)
        {
            processors.ForEach(Add);
        }

        public IEnumerable<string> InputTypes => typeMap.Keys;

        public void Add(IProcessor processor)
        {
            processors.Add(processor);
            foreach (var inputType in processor.InputTypes)
            {
                OnProcessorInputTypeAdded(processor, new InputTypeRegistrationEventArgs(inputType));
            }
            processor.InputTypeAdded += OnProcessorInputTypeAdded;
            processor.InputTypeRemoved += OnProcessorInputTypeRemoved;
        }

        private void OnProcessorInputTypeAdded(object sender, InputTypeRegistrationEventArgs e)
        {
            var processor = (IProcessor) sender;
            var inputType = e.DataType;
            if(typeMap.TryAdd(inputType, new List<IProcessor>()))
                InputTypeAdded?.Invoke(this, new InputTypeRegistrationEventArgs(inputType));

            if(!typeMap.TryGetValue(inputType, out var typeProcessors))
                throw new Exception($"Expected type map to contain '{inputType}'");
            lock (typeMapListLock)
            {
                if(typeProcessors.Contains(processor))
                    return;
                typeProcessors.Add(processor);
            }
        }

        private void OnProcessorInputTypeRemoved(object sender, InputTypeRegistrationEventArgs e)
        {
            var processor = (IProcessor) sender;
            var inputType = e.DataType;

            if(!typeMap.TryGetValue(inputType, out var typeProcessors))
                return;
            lock (typeMapListLock)
            {
                if (typeProcessors.Remove(processor))
                {
                    if (!typeProcessors.Any())
                    {
                        typeMap.TryRemove(inputType, out _);
                        InputTypeRemoved?.Invoke(this, new InputTypeRegistrationEventArgs(inputType));
                    }
                }
            }
        }

        public IEnumerable<IProcessor> GetAll()
        {
            return processors;
        }

        public IReadOnlyCollection<IProcessor> GetForType(string dataType)
        {
            if(!typeMap.ContainsKey(dataType))
                return new List<IProcessor>();
            return typeMap[dataType];
        }

        public event EventHandler<InputTypeRegistrationEventArgs> InputTypeAdded;
        public event EventHandler<InputTypeRegistrationEventArgs> InputTypeRemoved;
    }
}
