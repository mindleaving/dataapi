using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataProcessing.DataServices;
using DataProcessing.Models;
using DataProcessing.Objects;
using DataProcessing.Test.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test.DataServices
{
    [TestFixture]
    public class DataServiceProcessorTest : ApiTestBase
    {
        [Test]
        [Ignore("Tool")]
        [Category("Tool")]
        public async Task Run()
        {
            var username = "jdoe";
            var sut = new DataServiceProcessor(DataApiClient);
            var dataServiceDefinition = new DataServiceDefinition("1", username, nameof(Location), new List<DataServiceDefinition.Field>
            {
                new DataServiceDefinition.Field("Data.Site", "Site"),
                new DataServiceDefinition.Field("Data.Room", "Room")
            }, new SqlDataServiceTarget(IdGenerator.FromGuid(), "myserver", "MyDatabase", "Locations", username));
            var processorResult = await sut.Process(
                DataModificationType.Created,
                nameof(DataServiceDefinition),
                dataServiceDefinition.Id,
                JsonConvert.SerializeObject(dataServiceDefinition));
            Assert.That(processorResult.Status, Is.EqualTo(ProcessingStatus.Success));

            var location = new Location("MainSite", "2.31.14");
            processorResult = await sut.Process(DataModificationType.Replaced, nameof(Location), location.Id, JsonConvert.SerializeObject(location));
            Assert.That(processorResult.Status, Is.EqualTo(ProcessingStatus.Success), JsonConvert.SerializeObject(processorResult));
        }
    }
}
