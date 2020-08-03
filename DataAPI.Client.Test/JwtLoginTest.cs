using System.Net;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.UserManagement;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class JwtLoginTest
    {
        [Test]
        [Category("IntegrationTest")]
        public void CanLogInWithJwt()
        {
            var dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            Assume.That(dataApiClient.IsLoggedIn, Is.False);
            Assume.That(dataApiClient.LoggedInUsername, Is.Null);
            Assume.That(dataApiClient.IsAvailable());
            try
            {
                dataApiClient.Register("Test_sdfrgnbfgfdgj", "fgjerg", "sadfgfg", "egjdfbednbrgeo", "jdoe@example.org");
            }
            catch (ApiException apiException)
            {
                if (apiException.StatusCode != HttpStatusCode.Conflict)
                    throw;
            }
            var authenticationResult = dataApiClient.Login("Test_sdfrgnbfgfdgj", "egjdfbednbrgeo");
            Assert.That(authenticationResult, Is.Not.Null);
            Assert.That(authenticationResult.IsAuthenticated, Is.True);
            Assert.That(dataApiClient.IsLoggedIn, Is.True);
            Assert.That(dataApiClient.LoggedInUsername, Is.EqualTo("test_sdfrgnbfgfdgj"));

            Assert.That(() => authenticationResult = dataApiClient.Login("invalidUser", "invalidPassword"), Throws.Nothing);
            Assert.That(authenticationResult.IsAuthenticated, Is.False);
        }

        [Test]
        [Category("IntegrationTest")]
        public void CanSetAccessToken()
        {
            var dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            var accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InVuaXR0ZXN0YWRtaW4iLCJuYmYiOjE1NTY4ODExMzMsImV4cCI6MTU1Njg4NDczMywiaWF0IjoxNTU2ODgxMTMzfQ.Xe2m2KiROPMFaRt6s828ofMjNjAkEVHjHPccA5eqo2E";
            dataApiClient.SetAccessToken(accessToken);
            Assert.That(dataApiClient.IsLoggedIn, Is.True);
            Assert.That(dataApiClient.LoginMethod, Is.EqualTo(LoginMethod.JsonWebToken));
            Assert.That(dataApiClient.LoggedInUsername, Is.EqualTo("unittestadmin"));
        }
    }
}
