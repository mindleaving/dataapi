using System.Net.Http;
using DataAPI.Client.Test.Models;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    [Category("IntegrationTest")]
    public class SslCertificateErrorSuppressionTest
    {
        [Test]
        public void ValiddServerNameThrowsNoException()
        {
            var apiConfiguration = new ApiConfiguration("", 443);
            var dataApiClient = new DataApiClient(apiConfiguration);
            Assert.That(() => dataApiClient.ExistsAsync<Location>("someID"), Throws.Nothing);
        }

        [Test]
        public void InvalidServerNameThrowsExceptionIfNotSuppressed()
        {
            var apiConfiguration = new ApiConfiguration("", 443);
            var dataApiClient = new DataApiClient(apiConfiguration);
            Assert.That(() => dataApiClient.ExistsAsync<Location>("someID"), Throws.TypeOf<HttpRequestException>());
        }

        [Test]
        public void InvalidServerNameThrowsNoExceptionIfSuppressed()
        {
            var apiConfiguration = new ApiConfiguration("", 443);
            var dataApiClient = new DataApiClient(apiConfiguration, ignoreSslNameMismatch: true);
            Assert.That(() => dataApiClient.ExistsAsync<Location>("someID"), Throws.Nothing);
        }
    }
}
