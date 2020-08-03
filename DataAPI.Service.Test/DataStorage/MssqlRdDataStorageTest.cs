using System;
using System.Threading.Tasks;
using Commons.Misc;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Bson;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class MssqlRdDataStorageTest
    {
        [Test]
        public async Task CanReserveId()
        {
            var dataType = "SqlUnitTestObject";
            var id = "1234";
            var submitter = "jdoe";
            var sut = CreateMssqlRdDataStorage();
            var reservationResult = await sut.ReserveIdAsync(dataType, id, submitter);
            Assert.That(reservationResult.IsReserved, Is.True);
            Assert.That(reservationResult.Id, Is.EqualTo(id));
            await sut.DeleteDataContainerAsync(dataType, id);
        }

        [Test]
        public async Task CannotReserveIdTwice()
        {
            var dataType = "SqlUnitTestObject";
            var id = "1234";
            var submitter = "jdoe";
            var sut = CreateMssqlRdDataStorage();
            var reservationResult = await sut.ReserveIdAsync(dataType, id, submitter);
            Assert.That(reservationResult.IsReserved, Is.True);
            Assert.That(reservationResult.Id, Is.EqualTo(id));

            reservationResult = await sut.ReserveIdAsync(dataType, id, submitter);
            Assert.That(reservationResult.IsReserved, Is.False);
            await sut.DeleteDataContainerAsync(dataType, id);
        }

        [Test]
        public async Task CanCreateAndDeleteData()
        {
            var dataType = "SqlUnitTestObject";
            var utcNow = DateTime.UtcNow;
            var container = new GenericDataContainer(
                IdGenerator.FromGuid(),
                "jdoe",
                utcNow,
                "jdoe",
                utcNow,
                ApiVersion.Current,
                BsonDocument.Parse("{ Name : 'Jan', Address : { Street : 'Teglholm Tværvej', Number : 27 }}"));
            var sut = CreateMssqlRdDataStorage();
            StoreResult storeResult = null;
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, container, false), Throws.Nothing);
            Assert.That(storeResult.ModificationType, Is.EqualTo(DataModificationType.Created));
            var retrievedContainer = await sut.GetFromIdAsync(dataType, storeResult.Id);
            Assert.That(retrievedContainer.Submitter, Is.EqualTo(container.Submitter));
            Assert.That(retrievedContainer.SubmissionTimeUtc, Is.EqualTo(container.SubmissionTimeUtc).Within(TimeSpan.FromSeconds(1)));
            Assert.That(retrievedContainer.Data.GetValue("Name").AsString, Is.EqualTo(container.Data.GetValue("Name").AsString));

            // Cannot add document a second time
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, container, false), Throws.TypeOf<DocumentAlreadyExistsException>());

            // Can overwrite document
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, container, true), Throws.Nothing);
            Assert.That(storeResult.ModificationType, Is.EqualTo(DataModificationType.Replaced));

            var isDeleted = await sut.DeleteDataContainerAsync(dataType, storeResult.Id);
            Assert.That(isDeleted, Is.True);
        }

        private MssqlRdDataStorage CreateMssqlRdDataStorage()
        {
            return new MssqlRdDataStorage(
                DataStorageTypes.GenericSQL,
                "myserver",
                "DataApiSqlIntegrationUnitTest",
                "jdoe_dataapi",
                Secrets.Get("DataAPI_SqlPassword_myserver"),
                new DummyIdGeneratorManager());
        }
    }
}
