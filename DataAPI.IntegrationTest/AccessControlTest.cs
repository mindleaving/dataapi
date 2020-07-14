using System.Collections.Generic;
using System.Net;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class AccessControlTest : ApiTestBase
    {
        [Test]
        public void CollectionCanBeProtectedAndUnprotected()
        {
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(1, analystDataApiClient);

            try
            {
                List<UnitTestSearchObject> searchObjects = null;
                Assume.That(
                    () => searchObjects = analystDataApiClient.GetManyAsync<UnitTestSearchObject>("").Result, 
                    Throws.Nothing);
                Assume.That(searchObjects.Count, Is.EqualTo(1));

                AssertStatusCode(
                    () => adminDataApiClient.SetCollectionOptions(
                        new CollectionOptions(nameof(UnitTestSearchObject)) { IsProtected = true}),
                    HttpStatusCode.OK, "Protect collection");

                AssertStatusCode(
                    () => analystDataApiClient.GetManyAsync<UnitTestSearchObject>("").Wait(),
                    HttpStatusCode.Unauthorized, "Get many from protected collection");

                AssertStatusCode(
                    () => adminDataApiClient.SetCollectionOptions(
                        new CollectionOptions(nameof(UnitTestSearchObject)) { IsProtected = false}),
                    HttpStatusCode.OK, "Unprotect collection");

                AssertStatusCode(
                    () => searchObjects = analystDataApiClient.GetManyAsync<UnitTestSearchObject>("").Result,
                    HttpStatusCode.OK, "Get many from unprotected collection");
                Assert.That(searchObjects.Count, Is.EqualTo(1));
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData, analystDataApiClient);
            }
        }

        [Test]
        public void GlobalPermissionsAreUsedForUnprotectedCollection()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var dataType = nameof(UnitTestSearchObject);
            // Make sure collection is not protected
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(dataType) { IsProtected = false });
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(1, adminDataApiClient);

            try
            {
                // Check that user has access
                List<UnitTestSearchObject> searchObjects = null;
                AssertStatusCode(
                    () => searchObjects = dataApiClient.GetManyAsync<UnitTestSearchObject>("").Result,
                    HttpStatusCode.OK, "Get many with authorized user");
                Assert.That(searchObjects.Count, Is.EqualTo(1));

                // Remove role
                adminDataApiClient.RemoveGlobalRoleFromUser(dataApiClient.LoggedInUsername, Role.Analyst);

                AssertStatusCode(
                    () => dataApiClient.GetManyAsync<UnitTestSearchObject>("").Wait(), 
                    HttpStatusCode.Unauthorized, "Get many with unauthorized user");
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData, adminDataApiClient);
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void CollectionPermissionsAreUsedForProtectedCollection()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            var dataType = nameof(UnitTestSearchObject);
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(dataType) { IsProtected = true});
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(1, adminDataApiClient);

            try
            {
                // Make sure our user is not already permitted to access data from this collection
                AssertStatusCode(
                    () => dataApiClient.GetManyAsync<UnitTestSearchObject>("").Wait(), 
                    HttpStatusCode.Unauthorized, "Get many with user without roles");

                adminDataApiClient.AddCollectionRoleToUser(dataApiClient.LoggedInUsername, Role.Analyst, dataType);

                // Check that user has access
                List<UnitTestSearchObject> searchObjects = null;
                AssertStatusCode(
                    () => searchObjects = dataApiClient.GetManyAsync<UnitTestSearchObject>("").Result,
                    HttpStatusCode.OK, "Get many with user with added role");
                Assert.That(searchObjects.Count, Is.EqualTo(1));

                // Remove access
                adminDataApiClient.RemoveCollectionRoleFromUser(dataApiClient.LoggedInUsername, Role.Analyst, dataType);

                // Check that user no longer has access
                AssertStatusCode(
                    () => dataApiClient.GetManyAsync<UnitTestSearchObject>("").Wait(),
                    HttpStatusCode.Unauthorized, "Get many with user with removed role");
            }
            finally 
            {
                adminDataApiClient.SetCollectionOptions(
                    new CollectionOptions(dataType) { IsProtected = false});
                SearchDataGenerator.DeleteData(submittedData, adminDataApiClient);
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void DataFromOtherUserCannotBeOverwrittenNorDeleted()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            // Make sure overwriting is disabled
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(nameof(UnitTestDataObject1)) { NonAdminUsersCanOverwriteData = false });

            var testObject = new UnitTestDataObject1();
            string objectId = null;
            Assert.That(() => objectId = analystDataApiClient.InsertAsync(testObject).Result, Throws.Nothing);
            Assert.That(objectId, Is.Not.Null);

            try
            {
                // Test 1: Overwrite
                var otherTestObject = new UnitTestDataObject1 { Id = objectId };
                AssertStatusCode(
                    () => analyst2DataApiClient.ReplaceAsync(otherTestObject, otherTestObject.Id).Wait(),
                    HttpStatusCode.Unauthorized);

                // Test 2: Delete
                AssertStatusCode(
                    () => analyst2DataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait(),
                    HttpStatusCode.Unauthorized);
                AssertStatusCode(
                    () => analystDataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait(),
                    HttpStatusCode.OK);
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
            }
        }

        [Test]
        public void DataFromOtherUserCanBeOverwrittenAndDeletedIfEnabled()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var analyst2DataApiClient);

            var testObject = new UnitTestDataObject1();
            string objectId = null;
            Assert.That(() => objectId = analystDataApiClient.InsertAsync(testObject).Result, Throws.Nothing);
            Assert.That(objectId, Is.Not.Null);

            // Enable overwriting of data
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(nameof(UnitTestDataObject1)) { NonAdminUsersCanOverwriteData = true });

            try
            {
                // Test 1: Delete
                // NOTE: Order of tests is important, because overwriting data
                // results in taking ownership of the object
                // and then be able to delete it, but we also want to 
                // test that we can delete even if the object isn't ours.
                AssertStatusCode(
                    () => analyst2DataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait(),
                    HttpStatusCode.OK, "Delete");

                // Test 2: Overwrite
                AssertStatusCode(
                    () => analyst2DataApiClient.ReplaceAsync(new UnitTestDataObject1(), objectId).Wait(),
                    HttpStatusCode.OK, "Replace");
            }
            finally
            {
                UserGenerator.DeleteUser(analyst2DataApiClient);
                analystDataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait();
                adminDataApiClient.SetCollectionOptions(
                    new CollectionOptions(nameof(UnitTestDataObject1)) { NonAdminUsersCanOverwriteData = false });
            }
        }
    }
}
