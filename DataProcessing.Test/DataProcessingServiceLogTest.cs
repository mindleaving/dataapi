using Commons.Extensions;
using Commons.Physics;
using DataProcessing.Logging;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataProcessing.Test
{
    [TestFixture]
    public class DataProcessingServiceLogTest
    {
        [Test]
        public void NullDetailsSerializationRoundTrip()
        {
            var logEntry = new DataProcessingServiceLog("Test", null);
            var json = JsonConvert.SerializeObject(logEntry);
            DataProcessingServiceLog roundtripLogEntry = null;
            Assert.That(() => roundtripLogEntry = JsonConvert.DeserializeObject<DataProcessingServiceLog>(json), Throws.Nothing);
            Assert.That(roundtripLogEntry.Message, Is.EqualTo("Test"));
        }

        [Test]
        public void ExecutionSummaryDetailsSerializationRoundTrip()
        {
            var logEntry = new DataProcessingServiceLog("Test", new ExecutionSummaryLogEntryDetails(
                DataProcessingServiceExecutorType.Processor,
                "TestProcessor",
                1.To(Unit.Second),
                false,
                false,
                "TestID",
                "TestType"));
            var json = JsonConvert.SerializeObject(logEntry);
            DataProcessingServiceLog roundtripLogEntry = null;
            Assert.That(() => roundtripLogEntry = JsonConvert.DeserializeObject<DataProcessingServiceLog>(json), Throws.Nothing);
            Assert.That(roundtripLogEntry.Message, Is.EqualTo("Test"));
            Assert.That(roundtripLogEntry.Details.Type, Is.EqualTo(nameof(ExecutionSummaryLogEntryDetails)));
            var logEntryDetails = (ExecutionSummaryLogEntryDetails)roundtripLogEntry.Details;
            Assert.That(logEntryDetails.ExecutorType, Is.EqualTo(DataProcessingServiceExecutorType.Processor));
            Assert.That(logEntryDetails.InputDataObjectId, Is.EqualTo("TestID"));
        }
    }
}
