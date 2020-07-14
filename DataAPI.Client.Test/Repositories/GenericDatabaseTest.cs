using DataAPI.Client.Repositories;
using DataAPI.Client.Test.Models;
using NUnit.Framework;

namespace DataAPI.Client.Test.Repositories
{
    [TestFixture]
    public class GenericDatabaseTest
    {
        private readonly IDataApiClient dataApiClient;

        public GenericDatabaseTest()
        {
            var apiConfiguration = new ApiConfiguration("", 443);
            dataApiClient = new DataApiClient(apiConfiguration);
            dataApiClient.Login();
        }

        [Test]
        [Category("IntegrationTest")]
        public void GetAllDoesntThrowException()
        {
            var sut = new GenericDatabase<Location>(dataApiClient);
            Assert.That(async () => await sut.GetAllAsync(), Throws.Nothing);
        }
    }
}
