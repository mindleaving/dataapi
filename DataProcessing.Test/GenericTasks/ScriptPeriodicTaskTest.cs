using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataProcessing.GenericTasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test.GenericTasks
{
    [TestFixture]
    public class ScriptPeriodicTaskTest
    {
        private const string DefinitionPath = "";

        [Test]
        [Category("Tool")]
        [Ignore("Tool")]
        public async Task Run()
        {
            var definition = JsonConvert.DeserializeObject<ScriptPeriodicTaskDefinition>(File.ReadAllText(DefinitionPath));
            var sut = new ScriptPeriodTask(definition);
            var executionResult = await sut.Action(CancellationToken.None);
            Assert.That(executionResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task PassingScriptReturnsAsSuccessful()
        {
            var definition = JsonConvert.DeserializeObject<ScriptPeriodicTaskDefinition>(File.ReadAllText(DefinitionPath));
            var sut = new ScriptPeriodTask(definition);
            var executionResult = await sut.Action(CancellationToken.None);
            Console.WriteLine(executionResult.Summary);
            Assert.That(executionResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task FailingScriptReturnsAsUnsuccessful()
        {
            var definition = JsonConvert.DeserializeObject<ScriptPeriodicTaskDefinition>(File.ReadAllText(DefinitionPath));
            var sut = new ScriptPeriodTask(definition);
            var executionResult = await sut.Action(CancellationToken.None);
            Console.WriteLine(executionResult.Summary);
            Assert.That(executionResult.IsSuccess, Is.False);
        }
    }
}
