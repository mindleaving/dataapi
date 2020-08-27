using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class DownloadCommunicatorTest
    {
        [Test]
        [Category("IntegrationTest")]
        public async Task DownloadFileReturnsFilename()
        {
            var dataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            dataApiClient.Login();

            var actual = await dataApiClient.Download("DataBlob", "DCF9C0E1FD926E2C6B5E4E9E18001B3F00EEB30B");

            Assert.That(actual.Filename, Is.EqualTo("excelReferenceCache.json"));
            var data = ToByteArray(actual.Stream);
            Assert.That(data, Is.Not.Empty);
        }

        private byte[] ToByteArray(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
