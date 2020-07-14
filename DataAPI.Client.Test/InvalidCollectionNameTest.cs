using DataAPI.DataStructures.Exceptions;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class InvalidCollectionNameTest
    {
        [Test]
        [Category("IntegrationTest")]
        public void InvalidCollectionNameThrowsException()
        {
            var dataApiClient = new DataApiClient(new ApiConfiguration("", 443));
            Assert.That(async () => await dataApiClient.InsertAsync("Invalid!", "{ 'Id' : '46431' }"), Throws.InstanceOf<ApiException>());
        }
    }
}
