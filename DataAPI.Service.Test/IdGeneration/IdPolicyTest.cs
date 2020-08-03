using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.PostBodies;
using DataAPI.Service.DataStorage;
using DataAPI.Service.IdGeneration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.Service.Test.IdGeneration
{
    [TestFixture]
    public class IdPolicyTest
    {
        private const string Submitter = "jdoe";

        [Test]
        public async Task IIdIdIsUsed()
        {
            var rdDataStorage = new Mock<IRdDataStorage>();
            var obj = new Location("Hørsholm", "2.31.14");
            var submitBody = new SubmitDataBody(nameof(Location), obj, false, "SubmitBodyID");
            submitBody = JsonConvert.DeserializeObject<SubmitDataBody>(JsonConvert.SerializeObject(submitBody));
            var sut = new DefaultIdPolicy();
            var actual = await sut.DetermineIdAsync(submitBody, Submitter, rdDataStorage.Object);
            Assert.That(actual.Id, Is.EqualTo(obj.Id));
            rdDataStorage.Verify(x => x.GetIdsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task SubmissionBodyIdIsUsed()
        {
            var rdDataStorage = new Mock<IRdDataStorage>();
            var obj = new ClassWithoutId("Bob");
            var submitBody = new SubmitDataBody(nameof(ClassWithoutId), obj, false, "SubmitBodyID");
            submitBody = JsonConvert.DeserializeObject<SubmitDataBody>(JsonConvert.SerializeObject(submitBody));
            var sut = new DefaultIdPolicy();
            var actual = await sut.DetermineIdAsync(submitBody, Submitter, rdDataStorage.Object);
            Assert.That(actual.Id, Is.EqualTo(submitBody.Id));
            rdDataStorage.Verify(x => x.GetIdsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task RdDataStorageIdIsUsed()
        {
            var rdDataStorageId = "RdDataStorageId";
            var idReservationResult = IdReservationResult.Success(rdDataStorageId, false);
            var rdDataStorage = new Mock<IRdDataStorage>();
            rdDataStorage.Setup(x => x.GetIdsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(new List<IdReservationResult> { idReservationResult }));
            var obj = new ClassWithoutId("Bob");
            var submitBody = new SubmitDataBody(nameof(ClassWithoutId), obj, false); // ! No ID provided !
            submitBody = JsonConvert.DeserializeObject<SubmitDataBody>(JsonConvert.SerializeObject(submitBody));
            var sut = new DefaultIdPolicy();
            var actual = await sut.DetermineIdAsync(submitBody, Submitter, rdDataStorage.Object);
            Assert.That(actual.Id, Is.EqualTo(rdDataStorageId));
            rdDataStorage.Verify(x => x.GetIdsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        private class ClassWithoutId
        {
            public ClassWithoutId(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private class Location : IId
        {
            public string Id { get; }
            public string Site { get; }
            public string Room { get; }

            public Location(string site, string room)
            {
                Id = $"{Site}_{Room}";
                Site = site;
                Room = room;
            }
        }
    }
}
