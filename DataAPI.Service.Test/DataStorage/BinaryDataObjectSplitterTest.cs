using System;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.Service.DataStorage;
using MongoDB.Bson;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    public class BinaryDataObjectSplitterTest
    {
        [Test]
        public void RoundtripPreservesData()
        {
            var sut = new BinaryDataObjectSplitter(nameof(DataBlob.Data));
            var id = IdGenerator.FromGuid();
            var dataBlob = new DataBlob(id, new byte[] {0x42, 0x43, 0x44}, "myFile.bin");
            var payload = DataEncoder.Encode(JsonConvert.SerializeObject(dataBlob));
            var utcNow = DateTime.UtcNow;
            var container = new GenericDataContainer(id, "jdoe", utcNow, "jdoe", utcNow, "1.5.9", payload);
            BinaryDataObjectSplitterResult result = null;
            Assert.That(() => result = sut.Split(container), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BinaryData, Is.EqualTo(dataBlob.Data));
            var containerWithoutBinaryData = result.ContainerWithoutBinaryData;
            Assert.That(containerWithoutBinaryData.Id, Is.EqualTo(container.Id));
            Assert.That(containerWithoutBinaryData.Submitter, Is.EqualTo(container.Submitter));
            Assert.That(containerWithoutBinaryData.SubmissionTimeUtc, Is.EqualTo(container.SubmissionTimeUtc));
            Assert.That(containerWithoutBinaryData.ApiVersion, Is.EqualTo(container.ApiVersion));
            Assert.That(containerWithoutBinaryData.Data, Is.Not.Null);
            Assert.That(containerWithoutBinaryData.Data.GetValue(nameof(DataBlob.Data)), Is.EqualTo(BsonString.Empty));

            var reconstructedContainer = sut.Reassemble(containerWithoutBinaryData, result.BinaryData);
            Assert.That(reconstructedContainer.Id, Is.EqualTo(container.Id));
            Assert.That(reconstructedContainer.Submitter, Is.EqualTo(container.Submitter));
            Assert.That(reconstructedContainer.SubmissionTimeUtc, Is.EqualTo(container.SubmissionTimeUtc));
            Assert.That(reconstructedContainer.ApiVersion, Is.EqualTo(container.ApiVersion));
            Assert.That(reconstructedContainer.Data, Is.Not.Null);
            var reconstructedDataBlob = JsonConvert.DeserializeObject<DataBlob>(reconstructedContainer.Data.ToJson());
            Assert.That(reconstructedDataBlob.Id, Is.EqualTo(dataBlob.Id));
            Assert.That(reconstructedDataBlob.Filename, Is.EqualTo(dataBlob.Filename));
            Assert.That(reconstructedDataBlob.Data, Is.EqualTo(dataBlob.Data));
        }
    }
}
