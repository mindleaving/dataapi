using System;
using System.Collections.Generic;
using System.Linq;
using DataAPI.DataStructures;
using DataProcessing.Logging;
using DataProcessing.Test.Models;
using NUnit.Framework;

namespace DataProcessing.Test
{
    [TestFixture]
    public class DistributorTest : ApiTestBase
    {
        [Test]
        public void DistributorReactsToStartStop()
        {
            var sut = CreateDistributor(Enumerable.Empty<IProcessor>());
            Assert.That(() => sut.Start(), Throws.Nothing, "Start distributor");
            Assert.That(sut.IsRunning, Is.True, "Is distributor running?");
            Assert.That(() => sut.Stop(), Throws.Nothing, "Stop distributor");
            Assert.That(sut.IsRunning, Is.False, "Is distributor running?");
        }

        [Test]
        public void DistributorProcessesObjects()
        {
            var processor = new UnitTestProcessor<UnitTestInputObject,UnitTestOutputObject>("UnitTestProcessor", UnitTestInputObject.CalculateResult);
            var sut = CreateDistributor(new []{ processor });
            sut.PollInterval = TimeSpan.FromSeconds(3);
            Assume.That(() => sut.Start(), Throws.Nothing);
            Assume.That(sut.IsRunning, Is.True, "Is distributor running?");

            var inputObject1 = new UnitTestInputObject(new List<double> { 1, 3, 9 });
            var inputObject2 = new UnitTestInputObject(new List<double> { 1, 3, 9 });
            var outputObjects = new List<UnitTestOutputObject>();
            try
            {
                PushNewObject(inputObject1);
                sut.PollNow();
                Assert.That(() => processor.ProcessedItemsCount, Is.EqualTo(1).After(1000, 250));
                outputObjects = GetOutputObjects();
                Assert.That(outputObjects.Count, Is.EqualTo(1));
                var outputObject = outputObjects.Single();
                Assert.That(outputObject.Result, Is.EqualTo(UnitTestInputObject.CalculateResult(inputObject1).Result));

                PushNewObject(inputObject2);
                Assert.That(() => processor.ProcessedItemsCount, Is.EqualTo(2).After((int)(sut.PollInterval+TimeSpan.FromSeconds(1)).TotalMilliseconds, 250));
            }
            finally
            {
                CleanupObjects(new []{inputObject1, inputObject2}.Select(x => x.Id), outputObjects.Select(x => x.Id));
                DataApiClient.UnsubscribeAllAsync(nameof(UnitTestInputObject)).Wait();
                sut.Stop();
            }

        }

        private List<UnitTestOutputObject> GetOutputObjects()
        {
            return DataApiClient.GetManyAsync<UnitTestOutputObject>("").Result;
        }

        private void PushNewObject(IId inputObject)
        {
            DataApiClient.InsertAsync(inputObject, inputObject.Id).Wait();
        }

        private void CleanupObjects(IEnumerable<string> inputObjectIds, IEnumerable<string> outputObjectIds)
        {
            foreach (var id in inputObjectIds)
            {
                DataApiClient.DeleteAsync<UnitTestInputObject>(id).Wait();
            }
            foreach (var id in outputObjectIds)
            {
                DataApiClient.DeleteAsync<UnitTestOutputObject>(id).Wait();
            }
        }

        private Distributor CreateDistributor(IEnumerable<IProcessor> processors)
        {
            var dataProcessingServiceLogger = new DataProcessingServiceLogger(DataApiClient);
            return new Distributor(DataApiClient, new ProcessorDatabase(processors), dataProcessingServiceLogger);
        }
    }
}
