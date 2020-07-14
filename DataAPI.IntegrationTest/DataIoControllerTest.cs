using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.UserManagement;
using DataAPI.IntegrationTest.DataObjects;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class DataIoControllerTest : ApiTestBase
    {
        [Test]
        public void SubmitRejectsDataWithInvalidAccessToken()
        {
            var noLoginDataApiClient = new DataApiClient(ApiSetup.ApiConfiguration) {LoginMethod = LoginMethod.JsonWebToken};
            var testObject = new UnitTestDataObject1();
            AssertStatusCode(
                () => noLoginDataApiClient.InsertAsync(testObject).Wait(),
                HttpStatusCode.Unauthorized);
        }

        [Test]
        public void LoggedInUserCanSubmitGetAndDeleteOwnData()
        {
            var testObject = new UnitTestDataObject1();
            string objectId = null;
            Assert.That(() => objectId = analystDataApiClient.InsertAsync(testObject).Result, Throws.Nothing);
            Assert.That(objectId, Is.Not.Null);
            bool? objectExists = null;
            AssertStatusCode(
                () => objectExists = analystDataApiClient.ExistsAsync<UnitTestDataObject1>(objectId).Result,
                HttpStatusCode.OK);
            Assert.That(objectExists, Is.True);
            UnitTestDataObject1 retreivedObject = null;
            Assert.That(() => retreivedObject = analystDataApiClient.GetAsync<UnitTestDataObject1>(objectId).Result, Throws.Nothing);
            Assert.That(retreivedObject.Id, Is.EqualTo(testObject.Id));
            Assert.That(retreivedObject.Timestamp, Is.EqualTo(testObject.Timestamp));
            Assert.That(() => analystDataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait(), Throws.Nothing);
            objectExists = null;
            AssertStatusCode(
                () => objectExists = analystDataApiClient.ExistsAsync<UnitTestDataObject1>(objectId).Result,
                HttpStatusCode.OK);
            Assert.That(objectExists, Is.False);
        }

        [Test]
        public void LoggedInUserCanSubmitGetAndDeleteJsonData()
        {
            var testObject = new UnitTestDataObject1();
            var json = JsonConvert.SerializeObject(testObject);
            var dataType = testObject.GetType().Name;
            string objectId = null;
            Assert.That(() => objectId = analystDataApiClient.InsertAsync(dataType, json).Result, Throws.Nothing);
            Assert.That(objectId, Is.Not.Null);
            bool? objectExists = null;
            AssertStatusCode(
                () => objectExists = analystDataApiClient.ExistsAsync<UnitTestDataObject1>(objectId).Result,
                HttpStatusCode.OK);
            Assert.That(objectExists, Is.True);
            string retreivedObjectJson = null;
            Assert.That(() => retreivedObjectJson = analystDataApiClient.GetAsync(dataType, objectId).Result, Throws.Nothing);
            var retreivedObject = JsonConvert.DeserializeObject<UnitTestDataObject1>(retreivedObjectJson);
            Assert.That(retreivedObject.Id, Is.EqualTo(testObject.Id));
            Assert.That(() => analystDataApiClient.DeleteAsync(dataType, objectId).Wait(), Throws.Nothing);
            objectExists = null;
            AssertStatusCode(
                () => objectExists = analystDataApiClient.ExistsAsync(dataType, objectId).Result,
                HttpStatusCode.OK);
            Assert.That(objectExists, Is.False);
        }

        [Test]
        public void CanSubmitWithoutId()
        {
            var dataType = "UnitTestDataObject1";
            var testObject = new {Name = "Jan", Age = 29};
            string objectId = null;
            AssertStatusCode(
                () => objectId = analystDataApiClient.InsertAsync(dataType, JsonConvert.SerializeObject(testObject)).Result,
                HttpStatusCode.OK);
            Assert.That(objectId, Is.Not.Null);
            analystDataApiClient.DeleteAsync(dataType, objectId).Wait();
        }

        [Test]
        public void DeleteManyDeletesExpectedItems()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.DataProducer, adminDataApiClient, out var otherUserDataApiClient);

            var testObject1 = new UnitTestDataObject1 { Id = "DeleteManyTest_DeleteMe_1"};
            var testObject2 = new UnitTestDataObject1 { Id = "DeleteManyTest_DeleteMe_2"};
            var testObject3 = new UnitTestDataObject1 { Id = "DeleteManyTest_DoNotDelete"};
            var testObject4 = new UnitTestDataObject1 { Id = "DeleteManyTest_DeleteMe_Otheruser"};

            analystDataApiClient.ReplaceAsync(testObject1, testObject1.Id);
            analystDataApiClient.ReplaceAsync(testObject2, testObject2.Id);
            analystDataApiClient.ReplaceAsync(testObject3, testObject3.Id);
            otherUserDataApiClient.ReplaceAsync(testObject4, testObject4.Id);

            Task.Delay(500).Wait(); // Work-around for test objects occasionaly not being available when query below is executed
            try
            {
                List<DeleteResult> deleteResults = null;
                AssertStatusCode(
                    () => deleteResults = analystDataApiClient.DeleteMany<UnitTestDataObject1>("Data.Id LIKE '%DeleteMe%'").Result,
                    HttpStatusCode.OK);
                Assert.That(deleteResults.Count, Is.EqualTo(3));
                Assert.That(deleteResults, Has.Exactly(2).Matches<DeleteResult>(x => x.IsDeleted));
                Assert.That(deleteResults, Has.Exactly(1).Matches<DeleteResult>(x => !x.IsDeleted));
                Assert.That(analystDataApiClient.ExistsAsync<UnitTestDataObject1>(testObject1.Id).Result, Is.False);
                Assert.That(analystDataApiClient.ExistsAsync<UnitTestDataObject1>(testObject2.Id).Result, Is.False);
                Assert.That(analystDataApiClient.ExistsAsync<UnitTestDataObject1>(testObject3.Id).Result, Is.True);
                Assert.That(analystDataApiClient.ExistsAsync<UnitTestDataObject1>(testObject4.Id).Result, Is.True);
            }
            finally
            {
                analystDataApiClient.DeleteAsync<UnitTestDataObject1>(testObject1.Id);
                analystDataApiClient.DeleteAsync<UnitTestDataObject1>(testObject2.Id);
                analystDataApiClient.DeleteAsync<UnitTestDataObject1>(testObject3.Id);
                otherUserDataApiClient.DeleteAsync<UnitTestDataObject1>(testObject4.Id);
                UserGenerator.DeleteUser(otherUserDataApiClient);
            }
        }

        [Test]
        public void InvalidDataTypeIsRejected()
        {
            var json = "{ 'Id': '164861' }";
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync("Invalid!", json).Wait(),
                HttpStatusCode.BadRequest, "Invalid!");
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync("Inval+id", json).Wait(),
                HttpStatusCode.BadRequest, "Inval+id");
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync("Inval@id", json).Wait(),
                HttpStatusCode.BadRequest, "Inval@id");
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync("Inval#id", json).Wait(),
                HttpStatusCode.BadRequest, "Inval#id");
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync("Invalid DataType", json).Wait(),
                HttpStatusCode.BadRequest, "Invalid DataType");
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\n")]
        public void InvalidIdIsRejected(string id)
        {
            var testObject = new UnitTestDataObject1
            {
                Id = id
            };
            AssertStatusCode(
                () => analystDataApiClient.InsertAsync(testObject, testObject.Id).Wait(),
                HttpStatusCode.BadRequest);
        }

        // See bug 2866
        [Test]
        public void ViewerCanGetAndSearchData()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Viewer, adminDataApiClient, out var viewerDataApiClient);

            var testObject = new UnitTestDataObject1();
            var objectId = analystDataApiClient.InsertAsync(testObject).Result;
            try
            {
                UnitTestDataObject1 retreivedObject = null;
                AssertStatusCode(
                    () => retreivedObject = viewerDataApiClient.GetAsync<UnitTestDataObject1>(objectId).Result, 
                    HttpStatusCode.OK, "Get");
                Assert.That(retreivedObject.Id, Is.EqualTo(testObject.Id));

                var query = $"SELECT * FROM {nameof(UnitTestDataObject1)}";
                AssertStatusCode(
                    () => viewerDataApiClient.SearchAsync(query, ResultFormat.Json).Wait(),
                    HttpStatusCode.OK, "Search");
            }
            finally
            {
                analystDataApiClient.DeleteAsync<UnitTestDataObject1>(objectId).Wait();
                UserGenerator.DeleteUser(viewerDataApiClient);
            }
        }

        [Test]
        public void SqlQueryWithCurlyBracketsIsRejected()
        {
            var query = "SELECT {unwind}, Name FROM MyCollection";
            AssertStatusCode(() => analystDataApiClient.SearchAsync(query, ResultFormat.Json).Wait(),
                HttpStatusCode.BadRequest);
        }

        [Test]
        public void GetManyReturnsExpectedObjects()
        {
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(5, analystDataApiClient);

            try
            {
                List<UnitTestSearchObject> allSearchObjects = null;
                Assert.That(
                    () => allSearchObjects = analystDataApiClient.GetManyAsync<UnitTestSearchObject>("").Result,
                    Throws.Nothing);
                Assert.That(allSearchObjects, Is.Not.Null);
                Assume.That(allSearchObjects.Count, Is.EqualTo(submittedData.Count), "Search data collection may not be clean. Remove all documents from the collection and run this test again");
                CollectionAssert.AreEquivalent(submittedData.Select(x => x.Id), allSearchObjects.Select(x => x.Id));
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData.Select(x => x.Id), analystDataApiClient);
            }
        }

        [Test]
        public void GetManyCanLimitNumberOfObjects()
        {
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(5, analystDataApiClient);

            var limit = 3u;
            try
            {
                List<UnitTestSearchObject> allSearchObjects = null;
                Assert.That(
                    () => allSearchObjects = analystDataApiClient.GetManyAsync<UnitTestSearchObject>(limit: limit).Result,
                    Throws.Nothing);
                Assert.That(allSearchObjects, Is.Not.Null);
                Assert.That(allSearchObjects.Count, Is.EqualTo(limit));
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData.Select(x => x.Id), analystDataApiClient);
            }
        }

        [Test]
        public async Task SqlQueryReturnsExpectedResult()
        {
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(5, analystDataApiClient);

            var resultFormat = ResultFormat.Json;
            try
            {
                // Test 1:
                var query1 = $"SELECT Data.Id AS Id FROM {nameof(UnitTestSearchObject)}";
                Stream resultStream = null;
                Assert.That(async () => resultStream = await analystDataApiClient.SearchAsync(query1, resultFormat), Throws.Nothing);
                var results = await Client.Serialization.SeachResultStreamExtensions.ReadAllSearchResultsAsync(resultStream);
                Assert.That(results.All(x => x.ContainsKey("_id")), "One or more results do not contain '_id' field");
                if(results.Count > submittedData.Count)
                {
                    Assert.Inconclusive($"WARNING: Collection '{nameof(UnitTestSearchObject)}' contains data from previous tests. " +
                                      "Delete all documents in these collections manually");
                }
                CollectionAssert.IsSubsetOf(submittedData.Select(x => x.Id), results.Select(row => row.Value<string>("_id")), "Test 1 failed");

                // Test 2:
                var query2 = $"SELECT Data.Id AS Id, Data.Products.Price AS Price FROM {nameof(UnitTestSearchObject)} ORDER BY Price";
                Assert.That(() => resultStream = analystDataApiClient.SearchAsync(query2, resultFormat).Result, Throws.Nothing);
                results = await Client.Serialization.SeachResultStreamExtensions.ReadAllSearchResultsAsync(resultStream);
                Assert.That(results.All(x => x.ContainsKey("_id")), "One or more results do not contain '_id' field");
                Assert.That(results.All(x => x.ContainsKey("Price")), "Results do not contain 'Price'-field");
                var expectedPrices = submittedData.SelectMany(x => x.Products).Select(product => product.Price).OrderBy(price => price).ToList();
                var actualPrices = results.Select(x => x.Value<double>("Price")).ToList();
                CollectionAssert.AreEqual(expectedPrices, actualPrices, "Test 2 failed");
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData.Select(x => x.Id), analystDataApiClient);
            }
        }

        [Test]
        [TestCase("SELECT * FROM TestCollection WHERE Data.NotExistingPropety IS NOT NULL AND (Data.Member.Child = 'test' OR Data.OtherProperty LIKE 'aafj%')")]
        public void SqlQueryIsUnderstood(string query)
        {
            var resultFormat = ResultFormat.Json;
            AssertStatusCode(
                () => analystDataApiClient.SearchAsync(query, resultFormat).Wait(),
                HttpStatusCode.OK);
        }

        [Test]
        public void CanGetCollectionInformation()
        {
            var collectionName = nameof(UnitTestSearchObject);
            CollectionInformation collectionInformation = null;
            AssertStatusCode(
                () => collectionInformation = analystDataApiClient.GetCollectionInformationAsync(collectionName).Result,
                HttpStatusCode.OK);
            Assert.That(collectionInformation, Is.Not.Null);
            Assert.That(collectionInformation.CollectionName, Is.EqualTo(collectionName));
        }

        [Test]
        public void CanListCollectionNames()
        {
            List<string> collectionNames = null;
            AssertStatusCode(
                () => collectionNames = analystDataApiClient.ListCollectionNamesAsync(includeHidden: true).Result,
                HttpStatusCode.OK);
            Assert.That(collectionNames, Is.Not.Null);
            Assert.That(collectionNames, Has.One.EqualTo(nameof(UnitTestSearchObject)));
        }

        [Test]
        public void HiddenCollectionsAreNotListedUnlessSpecified()
        {
            var collectionName = nameof(UnitTestSearchObject);
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(collectionName) {IsHidden = true});

            List<string> collectionNames = null;
            AssertStatusCode(
                // ReSharper disable once RedundantArgumentDefaultValue
                () => collectionNames = analystDataApiClient.ListCollectionNamesAsync(includeHidden: false).Result,
                HttpStatusCode.OK);
            Assert.That(collectionNames, Is.Not.Null);
            Assert.That(collectionNames, Has.None.EqualTo(collectionName));
        }

        [Test]
        public void CanListCollectionInformations()
        {
            List<CollectionInformation> collectionInformations = null;
            AssertStatusCode(
                () => collectionInformations = analystDataApiClient.ListCollectionsAsync(includeHidden: true).Result,
                HttpStatusCode.OK);
            Assert.That(collectionInformations, Is.Not.Null);
            Assert.That(collectionInformations, Has.One.Matches<CollectionInformation>(x => x.CollectionName == nameof(UnitTestSearchObject)));
        }

        [Test]
        public void NonAdminUserCannotRedirectData()
        {
            AssertStatusCode(
                () => analystDataApiClient.SetDataRedirection("RedirectedDataType", DataStorageTypes.MongoDB),
                HttpStatusCode.Unauthorized);
        }

        [Test]
        public void AdminUserCanRedirectData()
        {
            AssertStatusCode(
                () => adminDataApiClient.SetDataRedirection("RedirectedDataType", DataStorageTypes.MongoDB),
                HttpStatusCode.OK);
        }

        [Test]
        public void CanRedirectToMongoDB()
        {
            AssertStatusCode(
                () => adminDataApiClient.SetDataRedirection("RedirectedDataType", DataStorageTypes.MongoDB),
                HttpStatusCode.OK);
        }

        [Test]
        public void CanRedirectToFilesystem()
        {
            AssertStatusCode(
                () => adminDataApiClient.SetDataRedirection("RedirectedDataType", DataStorageTypes.FileSystem),
                HttpStatusCode.OK);
        }

        [Test]
        public void CanRedirectToAzureBlobStorage()
        {
            AssertStatusCode(
                () => adminDataApiClient.SetDataRedirection("RedirectedDataType", DataStorageTypes.AzureBlobStorage),
                HttpStatusCode.OK);
        }

        [Test]
        public void CannotRedirectDataToUndefined()
        {
            AssertStatusCode(
                () => adminDataApiClient.SetDataRedirection("RedirectedDataType", "Undefined"),
                HttpStatusCode.ServiceUnavailable);
        }

        [Test]
        public void HiddenCollectionIsNotListedUnlessSpecified()
        {
            var collectionName = nameof(UnitTestDataObject1);
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(collectionName) {IsHidden = false});
            var collections = analystDataApiClient.ListCollectionNamesAsync(
                includeHidden: false).Result;
            Assume.That(collections, Has.One.EqualTo(collectionName));
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(collectionName) {IsHidden = true});
            try
            {
                // Test 1: Do not include hidden collections
                collections = analystDataApiClient.ListCollectionNamesAsync(
                    includeHidden: false).Result;
                Assert.That(collections, Has.None.EqualTo(collectionName));

                // Test 2: Include hidden collections
                collections = analystDataApiClient.ListCollectionNamesAsync(
                    includeHidden: true).Result;
                Assert.That(collections, Has.One.EqualTo(collectionName));
            }
            finally
            {
                adminDataApiClient.SetCollectionOptions(
                    new CollectionOptions(collectionName) {IsHidden = false});
            }
        }

        [Test]
        public async Task CanUploadLargeDataBlobs()
        {
            var data = new byte[35 * 1024 * 1024];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0x42;
            }
            var dataBlob = new DataBlob(IdGenerator.FromGuid(), data);
            if (await analystDataApiClient.ExistsAsync<DataBlob>(dataBlob.Id))
                await adminDataApiClient.DeleteAsync<DataBlob>(dataBlob.Id);

            string actualId = null;
            AssertStatusCode(
                () => actualId = analystDataApiClient.ReplaceAsync(dataBlob, dataBlob.Id).Result,
                HttpStatusCode.OK);
            analystDataApiClient.DeleteAsync<DataBlob>(actualId).Wait();
        }

        [Test]
        public async Task CanCreateSubmissionTransferDataAndGet()
        {
            var submissionId = "UnitTest_CanCreateSubmissionTransferDataAndGet";
            var dataBlob = new DataBlob(submissionId, Enumerable.Repeat((byte)0x39, 800).ToArray(), "unittest.bin");
            if(await adminDataApiClient.ExistsAsync<DataBlob>(dataBlob.Id))
                await adminDataApiClient.DeleteAsync<DataBlob>(submissionId);

            AssertStatusCode(
                () => submissionId = analystDataApiClient.CreateSubmission(dataBlob, x => x.Data, dataBlob.Id).Result,
                HttpStatusCode.OK);
            try
            {
                AssertStatusCode(
                    () => analystDataApiClient.TransferSubmissionData(dataBlob, x => x.Data, submissionId).Wait(),
                    HttpStatusCode.OK);
                DataBlob metadata = null;
                AssertStatusCode(
                    () => metadata = analystDataApiClient.GetSubmissionMetadata<DataBlob>(submissionId).Result,
                    HttpStatusCode.OK);
                Assert.That(metadata, Is.Not.Null);
                Assert.That(metadata.Filename, Is.EqualTo(dataBlob.Filename));
                // TODO: Test binary data transfer. Corresponding method missing in IDataApiClient
            }
            finally
            {
                await adminDataApiClient.DeleteAsync<DataBlob>(submissionId);
            }
        }

        [Test]
        public void EmptyInClauseInSearchResultsInEmptyResult()
        {
            // Contrary to throwing an Internal Server Error exception
            var query = $"SELECT * FROM {nameof(UnitTestDataObject1)} WHERE Data.Id IN []";
            AssertStatusCode(
                () => analystDataApiClient.SearchAsync(query, ResultFormat.Json).Wait(),
                HttpStatusCode.OK);
        }

        [Test]
        public void CanSearchWithQueryable()
        {
            var submittedData = SearchDataGenerator.GenerateAndSubmitSearchData(5, analystDataApiClient);

            try
            {
                // Test 1:
                List<string> actual = null;
                var repository = new GenericDatabase<UnitTestSearchObject>(analystDataApiClient);
                Assert.That(() => actual = repository.OrderBy(x => x.Name).Select(x => x.Id).ToList(), Throws.Nothing);
                CollectionAssert.AreEqual(
                    submittedData.OrderBy(x => x.Name).Select(x => x.Id).ToList(),
                    actual);
            }
            finally 
            {
                SearchDataGenerator.DeleteData(submittedData.Select(x => x.Id), analystDataApiClient);
            }
        }
    }
}



