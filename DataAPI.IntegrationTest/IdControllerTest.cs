using System;
using System.Net;
using DataAPI.Client;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class IdControllerTest : ApiTestBase
    {
        [Test]
        public void AnonymousCannotGetIds()
        {
            var noLoginDataApiClient = new DataApiClient(ApiSetup.ApiConfiguration) {LoginMethod = LoginMethod.JsonWebToken};
            var dataType = nameof(UnitTestDataObject1);
            AssertStatusCode(
                () => noLoginDataApiClient.GetNewIdAsync(dataType).Wait(),
                HttpStatusCode.Unauthorized);
        }

        [Test]
        public void DataProducerCanGetId()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.DataProducer, adminDataApiClient, out var dataProducerApiClient);

            var dataType = nameof(UnitTestDataObject1);
            string id = null;
            AssertStatusCode(
                () => id = dataProducerApiClient.GetNewIdAsync(dataType).Result,
                HttpStatusCode.OK);
            Assert.That(id, Is.Not.Null);
            analystDataApiClient.DeleteAsync<UnitTestDataObject1>(id);
        }

        [Test]
        public void UserCanReserveId()
        {
            var id = Guid.NewGuid().ToString();
            AssertStatusCode(
                () => analystDataApiClient.ReserveIdAsync<UnitTestDataObject1>(id).Wait(),
                HttpStatusCode.OK);
            analystDataApiClient.DeleteAsync<UnitTestDataObject1>(id);
        }

        [Test]
        public void UserCannotReserveIdTwice()
        {
            var id = Guid.NewGuid().ToString();
            AssertStatusCode(
                () => analystDataApiClient.ReserveIdAsync<UnitTestDataObject1>(id).Wait(),
                HttpStatusCode.OK);
            AssertStatusCode(
                () => analystDataApiClient.ReserveIdAsync<UnitTestDataObject1>(id).Wait(),
                HttpStatusCode.Forbidden);
            analystDataApiClient.DeleteAsync<UnitTestDataObject1>(id);
        }

        [Test]
        public void OtherUserCannotSubmitObjectWithSameId()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataProducerApiClient);

            var dataType = nameof(UnitTestDataObject1);
            string id = null;
            AssertStatusCode(
                () => id = dataProducerApiClient.GetNewIdAsync(dataType).Result,
                HttpStatusCode.OK);
            Assert.That(id, Is.Not.Null);
            var testObject = new UnitTestDataObject1 { Id = id };
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync(testObject, id).Wait(),
                HttpStatusCode.Conflict);
            dataProducerApiClient.DeleteAsync<UnitTestDataObject1>(id);
        }
    }
}
