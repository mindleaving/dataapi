using System;
using System.IO;
using System.Linq;
using DataProcessing.GenericTasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test
{
    [TestFixture]
    public class PeriodicTaskRunnerTest : ApiTestBase
    {
        [Test]
        public void TaskIsLoadedFromDirectory()
        {
            var taskDefinitionDirectory = $@"C:\temp\{Guid.NewGuid()}";
            CreateTestTaskDefinition(taskDefinitionDirectory);
            var logger = new NullDataProcessingServiceLogger();
            var taskDatabase = new TaskDatabase();
            var sut = new PeriodicTasksRunner(DataApiClient, taskDatabase, logger);
            sut.Start();
            Assert.That(() => taskDatabase.Tasks.OfType<ScriptPeriodTask>().Count(), Is.EqualTo(1).After(1000, 100));
        }

        private static void CreateTestTaskDefinition(string taskDefinitionDirectory)
        {
            var taskDefinition = new ScriptPeriodicTaskDefinition("TestTask", TimeSpan.FromMinutes(1), @"C:\temp\nonExistingScript.bat", "");
            var json = JsonConvert.SerializeObject(taskDefinition);
            if (!Directory.Exists(taskDefinitionDirectory))
                Directory.CreateDirectory(taskDefinitionDirectory);
            File.WriteAllText(Path.Combine(taskDefinitionDirectory, "TestScript.json"), json);
        }
    }
}
