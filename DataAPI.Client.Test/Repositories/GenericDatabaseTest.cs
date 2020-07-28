using System.Linq;
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

        [Test]
        [Category("IntegrationTest")]
        public void GetManyDoesntThrowException()
        {
            var sut = new GenericDatabase<TestObject1>(dataApiClient);
            Assert.That(async () => (await sut.GetManyAsync("_id LIKE 'abc%'", limit: 10)).ToList(), Throws.Nothing);
        }
    }
}
