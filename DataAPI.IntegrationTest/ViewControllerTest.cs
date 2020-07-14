using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Views;
using DataAPI.IntegrationTest.DataObjects;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class ViewControllerTest : ApiTestBase
    {
        [Test]
        public void AnonymousUserCannotCreateView()
        {
            var query = "SELECT * FROM CheeseInfo";
            var expires = DateTime.UtcNow.AddMinutes(3);
            var noLoginDataApiClient = new DataApiClient(ApiSetup.ApiConfiguration) {LoginMethod = LoginMethod.JsonWebToken};
            AssertStatusCode(
                () => noLoginDataApiClient.CreateViewAsync(query, expires).Wait(), 
                HttpStatusCode.Unauthorized);
        }

        [Test]
        public void LoggedInUserCanCreateAccessAndDeleteOwnViews()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);
            var query = "SELECT * FROM CheeseInfo LIMIT 3";
            var expires = DateTime.UtcNow.AddMinutes(3);
            ViewInformation viewInformation = null;
            try
            {
                Assert.That(() => viewInformation = dataApiClient.CreateViewAsync(query, expires).Result, Throws.Nothing);
                Assert.That(viewInformation, Is.Not.Null);
                Assert.That(() => dataApiClient.GetViewAsync(viewInformation.ViewId, ResultFormat.Json).Wait(), Throws.Nothing);
                Assert.That(() => dataApiClient.DeleteViewAsync(viewInformation.ViewId).Wait(), Throws.Nothing);
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public void ViewQueriesWithPlaceholderInFromArgumentAreRejected()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);
            var query = "SELECT * FROM {placeholder} LIMIT 3";
            var expires = DateTime.UtcNow.AddMinutes(3);
            ViewInformation viewInformation = null;
            try
            {
                AssertStatusCode(
                    () => viewInformation = dataApiClient.CreateViewAsync(query, expires).Result,
                    HttpStatusCode.BadRequest);
            }
            catch
            {
                // If test fails and view is created, delete it again
                dataApiClient.DeleteViewAsync(viewInformation.ViewId).Wait();
            }
            finally
            {
                UserGenerator.DeleteUser(dataApiClient);
            }
        }

        [Test]
        public async Task ViewQueryParametersAreApplied()
        {
            UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out var dataApiClient);

            // Generate data for test
            var searchDataCount = 5;
            var limit = 2; // This is our view parameter. If view parameters work as expected, only 2 of the 5 search objects are returned.
            Assume.That(searchDataCount, Is.GreaterThan(limit));
            var searchData = SearchDataGenerator.GenerateAndSubmitSearchData(searchDataCount, dataApiClient);

            ViewInformation viewInformation = null;
            try
            {
                // Setup
                var viewQuery = $"SELECT Data.Id AS Id FROM {nameof(UnitTestSearchObject)} LIMIT {{limit}}";
                var expires = DateTime.UtcNow.AddMinutes(3);
                Assert.That(() => viewInformation = dataApiClient.CreateViewAsync(viewQuery, expires).Result, Throws.Nothing);
                Assert.That(viewInformation, Is.Not.Null);

                // Test 1: BadRequest response if no parameters provided
                var resultFormat = ResultFormat.Json;
                AssertStatusCode(
                    () => dataApiClient.GetViewAsync(viewInformation.ViewId, resultFormat).Wait(),
                    HttpStatusCode.BadRequest);

                // Test 2: View is correctly generated when all parameters are provided and parameters are correctly applied
                var viewParameters = new Dictionary<string, string> {{"limit", limit.ToString()}};
                Stream viewResult = null;
                Assert.That(
                    async () => viewResult = await dataApiClient.GetViewAsync(viewInformation.ViewId, resultFormat, viewParameters), 
                    Throws.Nothing);
                var resultTable = await Client.Serialization.SeachResultStreamExtensions.ReadAllSearchResultsAsync(viewResult);
                Assert.That(resultTable.Count, Is.EqualTo(limit));
            }
            finally
            {
                SearchDataGenerator.DeleteData(searchData, dataApiClient);
                if(viewInformation != null)
                    dataApiClient.DeleteViewAsync(viewInformation.ViewId).Wait();
                UserGenerator.DeleteUser(dataApiClient);
            }
        }
    }
}
