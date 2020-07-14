using System;
using System.Net;
using DataAPI.Client;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    public abstract class ApiTestBase
    {
        protected readonly IDataApiClient adminDataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
        protected IDataApiClient analystDataApiClient;
        protected AuthenticationResult analystAuthenticationResult;

        [OneTimeSetUp]
        public void LoginAdmin()
        {
            Assume.That(adminDataApiClient.IsAvailable());

            var adminUserAuthenticationResult = adminDataApiClient.Login(ApiSetup.UnitTestAdminUsername, ApiSetup.UnitTestAdminPassword);
            Assume.That(adminDataApiClient.LoginMethod, Is.EqualTo(LoginMethod.JsonWebToken));
            if(!adminUserAuthenticationResult.IsAuthenticated)
                throw new Exception("Could not log unit test admin in");
            analystAuthenticationResult = UserGenerator.RegisterAndLoginUserWithRole(Role.Analyst, adminDataApiClient, out analystDataApiClient);
        }

        [OneTimeTearDown]
        public void LogoutAdmin()
        {
            UserGenerator.DeleteUser(analystDataApiClient);
            adminDataApiClient.Logout();
        }

        public static void AssertStatusCode(Action apiAction, HttpStatusCode statusCode, string testDescription = null)
        {
            try
            {
                apiAction();
                if (statusCode != HttpStatusCode.OK)
                    Assert.Fail($"{testDescription}: Expected API-exception, but none thrown");
            }
            catch (AggregateException aggregateException)
            {
                Assert.That(aggregateException.InnerException, Is.TypeOf<ApiException>());
                var apiException = aggregateException.InnerException as ApiException;
                Assert.That(apiException.StatusCode, Is.EqualTo(statusCode), $"{testDescription}: {apiException.Message}");
            }
            catch (ApiException apiException)
            {
                Assert.That(apiException.StatusCode, Is.EqualTo(statusCode), $"{testDescription}: {apiException.Message}");
            }
        }

        public void MakeCollectionProtected(string collectionName)
        {
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(collectionName) {IsProtected = true});
        }

        public void MakeCollectionUnprotected(string collectionName)
        {
            adminDataApiClient.SetCollectionOptions(
                new CollectionOptions(collectionName) {IsProtected = false});
        }
    }
}
