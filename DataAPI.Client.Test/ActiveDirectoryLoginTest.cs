using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class ActiveDirectoryLoginTest
    {
        [Test]
        [Category("IntegrationTest")]
        public void CanLogInWithActiveDirectory()
        {
            var dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);

            Assume.That(dataApiClient.IsAvailable());
            Assume.That(dataApiClient.IsLoggedIn, Is.False);
            Assume.That(dataApiClient.LoggedInUsername, Is.Null);
            //Assert.That(() => dataApiClient.Login("Test_sdfrgnbfgfdgj", "egjdfbednbrgeo"), Throws.Nothing);
            var authenticationResult = dataApiClient.Login();
            Assert.That(authenticationResult, Is.Not.Null);
            Assert.That(authenticationResult.IsAuthenticated, Is.True);
            Assert.That(dataApiClient.IsLoggedIn, Is.True);
            Assert.That(dataApiClient.LoggedInUsername, Is.EqualTo("jdoe"));
        }
    }
}
