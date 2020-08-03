using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataProcessing.GenericProcessors
{
    public class ScriptProcessor : ProcessorBase, ISingleOutputProcessor
    {
        public const int MaximumExecuteTimeInMilliSeconds = 60 * 60 * 1000;

        public ScriptProcessor(ScriptProcessorDefinition definition)
            : base(nameof(definition.DisplayName), nameof(definition.InputType))
        {
            Definition = definition;
        }

        public ScriptProcessorDefinition Definition { get; }
        public string OutputTypeName => Definition.OutputType;
        private IList<DataModificationType> ModificationTypes { get; } = new[] {DataModificationType.Created, DataModificationType.Replaced};
        public bool IsOfInterest(DataModificationType modificationType, string inputId, string inputJson)
        {
            if (!ModificationTypes.Contains(modificationType))
                return false;
            return true;
        }


        public override async Task<IProcessorResult> Process(
            DataModificationType modificationType,
            string dataType,
            string inputId,
            string inputJson)
        {
            var inputFilePath = $@"C:\temp\{DisplayName}_input.json";
            File.WriteAllText(inputFilePath, inputJson);
            var processInfo = new ProcessStartInfo(Definition.ScriptPath, inputFilePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var process = System.Diagnostics.Process.Start(processInfo);

            var errorMessages = new List<string>();
            process.ErrorDataReceived += (sender, args) =>
            {
                if(!string.IsNullOrEmpty(args.Data))
                    errorMessages.Add(args.Data);
            };
            process.BeginErrorReadLine();
            var output = string.Empty;
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    output += args.Data;
            };
            process.BeginOutputReadLine();

            if(!await Task.Run(() => process.WaitForExit(MaximumExecuteTimeInMilliSeconds)))
            {
                process.Kill();
                return new ErrorProcessorResult($"Processor {DisplayName} didn't finish within {MaximumExecuteTimeInMilliSeconds} ms");
            }

            var isSuccess = process.ExitCode == 0;
            if(!isSuccess)
            {
                if(!errorMessages.Any())
                    return new ErrorProcessorResult("An unknown error occured");
                return new ErrorProcessorResult(
                    $"Errors: {Environment.NewLine}" +
                    $"{errorMessages.Aggregate((a,b) => a + Environment.NewLine + b)}");
            }
            return BuildProcessorResults(output);
        }

        private IProcessorResult BuildProcessorResults(string json)
        {
            var resultObjects = new List<SerializedObject>();
            var jToken = JToken.Parse(json);
            if (jToken.Type == JTokenType.Object)
            {
                var jObject = (JObject)jToken;
                var id = GetOrGenerateId(jObject);
                resultObjects.Add(new SerializedObject(id, OutputTypeName, jObject.ToString(Formatting.None)));
            }
            else if (jToken.Type == JTokenType.Array)
            {
                foreach (var child in jToken.Children())
                {
                    if(child.Type != JTokenType.Object)
                        throw new FormatException("Unexpected JSON format. Output must either be a JSON-object or a JSON-array of JSON-objects");
                    var jObject = (JObject) child;
                    var id = GetOrGenerateId(jObject);
                    resultObjects.Add(new SerializedObject(id, OutputTypeName, jObject.ToString(Formatting.None)));
                }
            }
            else
            {
                throw new FormatException("Unexpected JSON format. Output must either be a JSON-object or a JSON-array of JSON-objects");
            }
            return new SuccessProcessorResult("", true, resultObjects);
        }

        private static string GetOrGenerateId(JObject jObject)
        {
            var id = jObject.ContainsKey("Id")
                ? jObject["Id"].Value<string>()
                : IdGenerator.FromGuid();
            return id;
        }
    }
}
