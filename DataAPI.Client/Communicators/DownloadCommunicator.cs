using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;

namespace DataAPI.Client.Communicators
{
    internal static class DownloadCommunicator
    {
        public static async Task<DownloadStream> GetFile(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string dataType,
            string id)
        {
            var url = RequestUriBuilder.Build(configuration, "/Download/GetFile");
            url += $"?dataType={Uri.EscapeDataString(dataType)}";
            url += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.GetAsync(url);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var contentDispositionHeader = response.Content.Headers.ContentDisposition;
            var filename = StripQuotes(contentDispositionHeader?.FileName);
            return new DownloadStream(
                await response.Content.ReadAsStreamAsync(),
                filename);
        }

        private static string StripQuotes(string fileName)
        {
            if (fileName == null)
                return null;
            if (Regex.IsMatch(fileName, "^[\"'].*[\"']$"))
                return fileName.Substring(1, fileName.Length - 2);
            return fileName;
        }
    }
}
