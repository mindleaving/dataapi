using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using NUnit.Framework;

namespace DataAPI.IntegrationTest
{
    [TestFixture]
    public class DownloadControllerTest : ApiTestBase
    {
        [Test]
        public async Task CanDownloadDataBlobWithDataTypeIdPair()
        {
            var dataBlob = new DataBlob(IdGenerator.FromGuid(), new byte[] {0x01, 0x02, 0x03}, "testFile.bin");
            Console.WriteLine(dataBlob.Id);
            if(!await analystDataApiClient.ExistsAsync<DataBlob>(dataBlob.Id))
                await analystDataApiClient.InsertAsync(dataBlob, dataBlob.Id);
            try
            {
                var httpHandler = new HttpClientHandler();
                var httpClient = new HttpClient(httpHandler);
                var accessToken = analystAuthenticationResult.AccessToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var uri = RequestUriBuilder.Build(ApiSetup.ApiConfiguration, "download/getFile");
                uri += $"?dataType={Uri.EscapeDataString(nameof(DataBlob))}&id={Uri.EscapeDataString(dataBlob.Id)}";
                var response = await httpClient.GetAsync(uri);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content.Headers.ContentDisposition, Is.Not.Null);
                var content = await response.Content.ReadAsByteArrayAsync();
                Assert.That(content, Is.EqualTo(dataBlob.Data));
            }
            finally
            {
                await TryDeleteDataBlob(dataBlob.Id);
            }
        }

        [Test]
        public async Task CanDownloadDataBlobWithShortId()
        {
            var dataBlob = new DataBlob(IdGenerator.FromGuid(), new byte[] {0x01, 0x02, 0x03}, "testFile.bin");
            var shortId = new ShortId("unittest_datablob", nameof(DataBlob), dataBlob.Id);
            if(!await analystDataApiClient.ExistsAsync<DataBlob>(dataBlob.Id))
                await analystDataApiClient.InsertAsync(dataBlob, dataBlob.Id);
            if(!await analystDataApiClient.ExistsAsync<ShortId>(shortId.Id))
                await analystDataApiClient.InsertAsync(shortId, shortId.Id);
            try
            {
                var httpHandler = new HttpClientHandler();
                var httpClient = new HttpClient(httpHandler);
                var accessToken = analystAuthenticationResult.AccessToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var uri = RequestUriBuilder.Build(ApiSetup.ApiConfiguration, "download/getFile");
                uri += $"?shortId={Uri.EscapeDataString(shortId.Id)}";
                var response = await httpClient.GetAsync(uri);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content.Headers.ContentDisposition, Is.Not.Null);
                var content = await response.Content.ReadAsByteArrayAsync();
                Assert.That(content, Is.EqualTo(dataBlob.Data));
            }
            finally
            {
                await analystDataApiClient.DeleteAsync<ShortId>(shortId.Id);
                await TryDeleteDataBlob(dataBlob.Id);
            }
        }

        private async Task TryDeleteDataBlob(string dataBlobId)
        {
            try
            {
                await adminDataApiClient.DeleteAsync<DataBlob>(dataBlobId);
            }
            catch
            {
                Console.WriteLine($"Cannot delete test data blob with ID '{dataBlobId}'. Delete it manually.");
            }
        }
    }
}
