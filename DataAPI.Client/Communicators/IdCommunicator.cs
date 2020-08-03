using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DataAPI.Client.Communicators
{
    internal static class IdCommunicator
    {
        public static async Task<List<string>> GetIdsAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, uint count)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/id/getnew");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&count={count}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var content = await response.Content.ReadAsStringAsync();
            return content.Split('\n').Select(x => x.Trim()).ToList();
        }

        public static async Task<string> GetIdAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType)
        {
            var ids = await GetIdsAsync(configuration, httpClient, dataType, 1);
            return ids.First();
        }

        public static async Task ReserveIdAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/id/reserve");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }
    }
}
