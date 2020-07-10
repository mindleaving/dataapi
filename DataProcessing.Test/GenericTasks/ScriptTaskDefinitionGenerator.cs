using System;
using System.IO;
using DataProcessing.GenericTasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test.GenericTasks
{
    [TestFixture]
    public class ScriptTaskDefinitionGenerator
    {
        [Test]
        [Category("Tool")]
        [Ignore("Tool")]
        public void GeneratePeriodicTaskDefinition()
        {
            var outputFilePath = $@"C:\temp\{nameof(ScriptPeriodicTaskDefinition)}.json";
            var displayName = "";
            var period = TimeSpan.Zero;
            var scriptPath = "";
            var arguments = "";
            var scriptPeriodicTaskDefinition = new ScriptPeriodicTaskDefinition(
                displayName,
                period,
                scriptPath,
                arguments);
            var json = JsonConvert.SerializeObject(scriptPeriodicTaskDefinition, Formatting.Indented);
            File.WriteAllText(outputFilePath, json);
        }
    }
}
