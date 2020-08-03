using System.Collections.Generic;
using System.IO;
using DataProcessing.GenericProcessors;
using Newtonsoft.Json;

namespace DataProcessing
{
    public class ProcessorLoader
    {
        private readonly string processorDefinitionDirectory;

        public ProcessorLoader(string processorDefinitionDirectory)
        {
            this.processorDefinitionDirectory = processorDefinitionDirectory;
        }

        public IList<IProcessor> Load()
        {
            if (!Directory.Exists(processorDefinitionDirectory))
            {
                try
                {
                    Directory.CreateDirectory(processorDefinitionDirectory);
                }
                catch
                {
                    // ignore
                }
                return new List<IProcessor>();
            }
            var jsonFiles = Directory.EnumerateFiles(processorDefinitionDirectory, "*.json");
            var processors = new List<IProcessor>();
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var processorDefinition = JsonConvert.DeserializeObject<ScriptProcessorDefinition>(json);
                    var processor = new ScriptProcessor(processorDefinition);
                    processors.Add(processor);
                }
                catch
                {
                    // ignored
                }
            }
            return processors;
        }
    }
}
