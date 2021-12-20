using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.Objects;
using DataProcessing.ProcessingPostponing;
using DataProcessing.Test.Models;
using Moq;
using NUnit.Framework;

namespace DataProcessing.Test.PostponedProcessing
{
    [TestFixture]
    public class PostponedProcessingIntegrationTest
    {
        [Test]
        public async Task PostponedProcessingRunnerIsNotifiedOfPostponedAnalysis()
        {
            var dataApiSimulator = new DataApiSimulator();
            dataApiSimulator.DataApiClientMock
                .Setup(x => x.GetManyAsync<PostponedProcessingObject>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<uint?>()))
                .Returns<string, string, int>((filter, orderBy, limit) => Task.FromResult(dataApiSimulator.GetMany<PostponedProcessingObject>(filter, limit)));
            dataApiSimulator.DataApiClientMock
                .Setup(x => x.DeleteAsync<PostponedProcessingObject>(It.IsAny<string>()))
                .Callback<string>(dataType => dataApiSimulator.Delete<PostponedProcessingObject>(dataType))
                .Returns(Task.CompletedTask);
            // Processor 1: Postpones processing, later completes processing when missing data is available
            var unitTestProcessor1 = new UnitTestProcessor<UnitTestInputObject, UnitTestOutputObject>("P1", UnitTestInputObject.CalculateResult);
            // Processor 2: Produces data that is needed by processor 1
            var unitTestProcessor2 = new UnitTestProcessor<P2InputData, ProcessedData>("P2", x => new ProcessedData(x.Id));
            var processors = new IProcessor[]
            {
                unitTestProcessor1,
                unitTestProcessor2,
                new PostponedProcessingObjectUpdateProcessor(dataApiSimulator.DataApiClient)
            };
            var postponedProcessingRunner = new PostponedProcessingRunner(
                dataApiSimulator.DataApiClient,
                processors,
                new NullDataProcessingServiceLogger());
            var processorDatabase = new ProcessorDatabase(processors.Concat(new []{ postponedProcessingRunner }));
            var distributor = new Distributor(
                dataApiSimulator.DataApiClient,
                processorDatabase,
                new NullDataProcessingServiceLogger());
            distributor.Start();
            postponedProcessingRunner.Start();
            Assert.That(
                await dataApiSimulator.DataApiClient.GetSubscriptionsAsync(), 
                Has.One.Matches<SubscriptionInfo>(x => x.DataType == unitTestProcessor1.InputTypes.First()));
            Assert.That(
                await dataApiSimulator.DataApiClient.GetSubscriptionsAsync(), 
                Has.One.Matches<SubscriptionInfo>(x => x.DataType == unitTestProcessor2.InputTypes.First()));
            Assert.That(
                await dataApiSimulator.DataApiClient.GetSubscriptionsAsync(), 
                Has.One.Matches<SubscriptionInfo>(x => x.DataType == nameof(PostponedProcessingObject)));

            var missingDataId = "FakeID";
            var missingDataReference = new DataReference(unitTestProcessor2.OutputTypeName, missingDataId);
            unitTestProcessor1.ActivatePostponing(missingDataReference);

            var inputObject1 = new UnitTestInputObject(new List<double> {42d});
            await dataApiSimulator.DataApiClient.InsertAsync(inputObject1);

            try
            {
                distributor.PollNow();
                Assert.That(() => unitTestProcessor1.PostponedItemsCount, Is.EqualTo(1).After(10*1000, 200), "Postponed item count expected == 1");
                distributor.PollNow();
                Assert.That(
                    () => postponedProcessingRunner.PostponedObjectCount, 
                    Is.EqualTo(1).After(10*1000, 200), 
                    $"Is {nameof(PostponedProcessingRunner)} aware of postponed object?");
                Assert.That(postponedProcessingRunner.PostponedObjectCount, Is.EqualTo(1));
                Assert.That(
                    await dataApiSimulator.DataApiClient.GetSubscriptionsAsync(), 
                    Has.One.Matches<SubscriptionInfo>(x => x.DataType == missingDataReference.DataType));

                unitTestProcessor1.DeativatePostponing();
                await dataApiSimulator.DataApiClient.InsertAsync(new P2InputData(missingDataReference.Id));
                distributor.PollNow();
                Assert.That(
                    () => unitTestProcessor2.ProcessedItemsCount, 
                    Is.EqualTo(1).After(10*1000, 200), 
                    "Processor 2: Processed item count expected == 1");
                Assert.That(
                    await dataApiSimulator.DataApiClient.ExistsAsync(missingDataReference.DataType, missingDataReference.Id), 
                    Is.True.After(1000, 200), 
                    "Processed data exists?");
                distributor.PollNow();
                await Task.Delay(3000);
                postponedProcessingRunner.PollNow();

                Assert.That(() => unitTestProcessor1.ProcessedItemsCount, Is.EqualTo(1).After(30*1000, 200), "Processor 1: Processed item count expected == 1");
                distributor.PollNow();
                Assert.That(
                    () => postponedProcessingRunner.PostponedObjectCount, 
                    Is.EqualTo(0).After(10*1000, 200), 
                    $"Is {nameof(PostponedProcessingRunner)} tracking any items?");

                // Test that no additional/repeated processing is triggered
                unitTestProcessor1.ResetProcessedItemsCount();
                unitTestProcessor2.ResetProcessedItemsCount();
                for (int i = 0; i < 3; i++)
                {
                    distributor.PollNow();
                    postponedProcessingRunner.PollNow();
                    await Task.Delay(1000);
                }
                Assert.That(unitTestProcessor1.ProcessedItemsCount, Is.EqualTo(0));
                Assert.That(unitTestProcessor2.ProcessedItemsCount, Is.EqualTo(0));
                Assert.That(postponedProcessingRunner.PostponedObjectCount, Is.EqualTo(0));
            }
            finally
            {
                distributor.Stop();
                postponedProcessingRunner.Stop();
            }
        }

        private class P2InputData : IId
        {
            public P2InputData(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }
        private class ProcessedData : IId
        {
            public ProcessedData(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }
    }
}
