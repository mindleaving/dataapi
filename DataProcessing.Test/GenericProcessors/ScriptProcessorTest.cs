using System;
using System.IO;
using System.Linq;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.GenericProcessors;
using DataProcessing.Objects;
using DataProcessing.Test.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test.GenericProcessors
{
    [TestFixture]
    public class ScriptProcessorTest
    {
        [Test]
        // TODO: [TestCase()]
        public void ValidJsonObjectIsProcessed(string definitionPath, string inputJsonFilePath)
        {
            var inputJson = File.ReadAllText(inputJsonFilePath);
            var definition = JsonConvert.DeserializeObject<ScriptProcessorDefinition>(File.ReadAllText(definitionPath));
            var sut = new ScriptProcessor(definition);
            IProcessorResult processorResult = null;
            Assert.That(async () => processorResult = await sut.Process(DataModificationType.Created, nameof(UnitTestInputObject), "FakeID", inputJson), Throws.Nothing);
            Assert.That(processorResult, Is.Not.Null.And.TypeOf<SuccessProcessorResult>());
            var outputObjects = ((SuccessProcessorResult)processorResult).Objects;
            Assert.That(outputObjects, Is.Not.Null);
            Assert.That(outputObjects.Count, Is.EqualTo(1));
            var outputObject = outputObjects.Single();
            Assert.That(outputObject.DataType, Is.EqualTo(definition.OutputType));
            Assert.That(outputObject.Json, Is.Not.Empty);
            Console.WriteLine(outputObject.Json);
        }
    }
}
