using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;

namespace DataAPI.Service.Test.DataStorage
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class ExistingMssqlTablesRdDataStorageTest
    {
        [Test]
        public async Task CanGetId()
        {
            var dataType = "SqlUnitTestObject";
            var submitter = "jdoe";
            var sut = CreateMssqlRdDataStorage(out _);
            var reservationResult = (await sut.GetIdsAsync(dataType, submitter, 1)).First();
            Assert.That(reservationResult.IsReserved, Is.True);
            Assert.That(reservationResult.Id, Is.Not.Null);
            await sut.DeleteDataContainerAsync(dataType, reservationResult.Id);
        }

        [Test]
        public async Task CanCreateAndDeleteData()
        {
            var dataType = "SqlUnitTestObject";
            var utcNow = DateTime.UtcNow;
            var sut = CreateMssqlRdDataStorage(out _);
            var submitter = "jdoe";
            var container = new GenericDataContainer(
                "-1",
                submitter,
                utcNow,
                submitter,
                utcNow,
                ApiVersion.Current,
                BsonDocument.Parse("{ "
                                   + $"Id : '{IdGenerator.FromGuid()}', "
                                   + $"OriginalSubmitter: 'jdoe', "
                                   + $"CreatedTimeUtc: '{utcNow:yyyy-MM-dd HH:mm:ss}', "
                                   + $"Submitter: 'jdoe', "
                                   + $"SubmissionTimeUtc: '{utcNow:yyyy-MM-dd HH:mm:ss}', "
                                   + $"'Data#Name': 'Bertha' "
                                   + "}"));
            
            StoreResult storeResult = null;
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, container, true), Throws.Nothing);
            Assert.That(storeResult.ModificationType, Is.EqualTo(DataModificationType.Created));
            var retrievedContainer = await sut.GetFromIdAsync(dataType, storeResult.Id);
            Assert.That(retrievedContainer.Submitter, Is.EqualTo(container.Submitter));
            Assert.That(retrievedContainer.SubmissionTimeUtc, Is.EqualTo(container.SubmissionTimeUtc).Within(TimeSpan.FromSeconds(1)));
            Assert.That(retrievedContainer.Data.GetValue("Data.Name").AsString, Is.EqualTo(container.Data.GetValue("Data#Name").AsString));

            // Cannot add document a second time
            var idReplacedContainer = new GenericDataContainer(
                storeResult.Id,
                container.OriginalSubmitter,
                container.CreatedTimeUtc,
                container.Submitter,
                container.SubmissionTimeUtc,
                container.ApiVersion,
                container.Data);
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, idReplacedContainer, false), Throws.TypeOf<DocumentAlreadyExistsException>());

            // Can overwrite document
            Assert.That(async () => storeResult = await sut.StoreAsync(dataType, idReplacedContainer, true), Throws.Nothing);
            Assert.That(storeResult.ModificationType, Is.EqualTo(DataModificationType.Replaced));

            var isDeleted = await sut.DeleteDataContainerAsync(dataType, storeResult.Id);
            Assert.That(isDeleted, Is.True);
        }

        [Test]
        public async Task CanGetManyComponents()
        {
            var sut = CreateMssqlRdDataStorage(out _);
            var components = sut.GetManyAsync("Component", "Data.business_name <> ''", "", limit: 10);
            await foreach (var component in components)
            {
                Assert.That(component, Is.Not.Null);
            }
        }

        [Test]
        public async Task CanSearchComponents()
        {
            var sut = CreateMssqlRdDataStorage(out _);
            var query = new DataApiSqlQuery(
                fromArguments: "Component",
                selectArguments: "*",
                whereArguments: "Data.business_name <> ''",
                limitArguments: "10");
            var components = sut.SearchAsync(query);
            await foreach (var component in components)
            {
                Assert.That(component, Is.Not.Null);
                Assert.That(component["Data"], Is.Not.Null.And.Not.EqualTo(BsonNull.Value));
            }

            query = new DataApiSqlQuery(
                fromArguments: "Component",
                selectArguments: "Data.id as ID, Data.source_system AS SourceSystem, Data.source_id",
                limitArguments: "3",
                whereArguments: "Data.created_by = 'auser'");
            components = sut.SearchAsync(query);
            await foreach (var component in components)
            {
                Assert.That(component, Is.Not.Null);
                Assert.That(component["ID"], Is.Not.Null.And.Not.EqualTo(BsonNull.Value));
                Assert.That(component["SourceSystem"], Is.Not.Null.And.Not.EqualTo(BsonNull.Value));
            }
        }

        private ExistingMssqlTablesRdDataStorage CreateMssqlRdDataStorage(out Mock<IRdDataStorage> metadataStorage)
        {
            metadataStorage = new Mock<IRdDataStorage>();
            var containers = new Dictionary<string, GenericDataContainer>();
            metadataStorage
                .Setup(x => x.StoreAsync(It.IsAny<string>(), It.IsAny<GenericDataContainer>(), It.IsAny<bool>()))
                .Callback<string,GenericDataContainer,bool>((dataType, container, overwrite) => containers[container.Id] = container)
                .Returns<string,GenericDataContainer,bool>((dataType, container, overwrite) => Task.FromResult(new StoreResult(container.Id, DataModificationType.Created, false)));
            metadataStorage
                .Setup(x => x.GetFromIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((dataType, id) => Task.FromResult(containers.ContainsKey(id) ? containers[id] : null));
            var tableSetups = new List<SqlTableSetup>
            {
                new SqlTableSetup(
                    "SqlUnitTestObject", 
                    "myserver",
                    "[DataApiSqlIntegrationUnitTest].[dbo].[SqlUnitTestObject]",
                    "jdoe_dataapi",
                    "DataAPI_SqlPassword_myserver",
                    "SqlId"),
                new SqlTableSetup(
                    "Component", 
                    "myserver",
                    "[DataApiSqlIntegrationUnitTest].[dbo].[Component]",
                    "jdoe_dataapi",
                    "DataAPI_SqlPassword_myserver",
                    "id"),
            };
            return new ExistingMssqlTablesRdDataStorage(
                DataStorageTypes.ExistingSQL,
                metadataStorage.Object,
                tableSetups);
        }
    }
}
