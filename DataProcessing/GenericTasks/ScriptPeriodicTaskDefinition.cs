using System;
using Newtonsoft.Json;

namespace DataProcessing.GenericTasks
{
    public class ScriptPeriodicTaskDefinition
    {
        [JsonConstructor]
        public ScriptPeriodicTaskDefinition(
            string displayName,
            TimeSpan period,
            string scriptPath,
            string arguments)
        {
            DisplayName = displayName;
            Period = period;
            ScriptPath = scriptPath;
            Arguments = arguments;
        }

        public string DisplayName { get; private set; }
        public TimeSpan Period { get; private set; }
        public string ScriptPath { get; private set; }
        public string Arguments { get; private set; }
    }
}
