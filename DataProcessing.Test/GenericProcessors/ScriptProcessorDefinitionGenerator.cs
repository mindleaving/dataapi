using System.Collections.Generic;
using System.IO;
using DataProcessing.GenericProcessors;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test.GenericProcessors
{
    [TestFixture]
    public class ScriptProcessorDefinitionGenerator
    {
        [Test]
        [Category("Tool")]
        [Ignore("Tool")]
        public void Generate()
        {
            var displayName = "TestScript";
            var inputType = "";
            var outputType = "";
            var scriptPath = "";
            var parameters = new Dictionary<string, string>
            {
                {"TestParameter", "TestValue"}
            };
            var outputFilePath = $@"C:\temp\{nameof(ScriptProcessorDefinition)}.json";
            var scriptProcessorDefinition = new ScriptProcessorDefinition(
                displayName,
                inputType,
                outputType,
                scriptPath,
                parameters);
            var json = JsonConvert.SerializeObject(scriptProcessorDefinition, Formatting.Indented);
            File.WriteAllText(outputFilePath, json);
        }
    }
}
