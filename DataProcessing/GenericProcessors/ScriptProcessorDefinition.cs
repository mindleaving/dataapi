using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataProcessing.GenericProcessors
{
    public class ScriptProcessorDefinition
    {
        [JsonConstructor]
        public ScriptProcessorDefinition(
            string displayName,
            string inputType,
            string outputType,
            string scriptPath, 
            Dictionary<string, string> parameters)
        {
            InputType = inputType;
            OutputType = outputType;
            ScriptPath = scriptPath;
            Parameters = parameters;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }
        public string InputType { get; private set; }
        public string OutputType { get; private set; }
        public string ScriptPath { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
    }
}
