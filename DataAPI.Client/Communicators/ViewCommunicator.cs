using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.Views;

namespace DataAPI.Client.Communicators
{
    internal static class ViewCommunicator
    {
        public static async Task<ViewInformation> CreateViewAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string query,
            DateTime expires,
            string viewId = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/view/create");
            var body = ConstructCreateViewBody(query, expires, viewId);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return ConfiguredJsonSerializer.Deserialize<ViewInformation>(json);
        }

        private static HttpContent ConstructCreateViewBody(string query, DateTime expires, string viewId = null)
        {
            var createViewBody = new CreateViewBody(query, expires, viewId);
            return PostBodyBuilder.ConstructBody(createViewBody);
        }

        public static async Task<Stream> GetViewAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient, 
            string viewId,
            ResultFormat resultFormat,
            Dictionary<string, string> parameters = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/view/get");
            requestUri += BuildGetQuery(viewId, parameters, resultFormat);
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStreamAsync();
        }

        private static string BuildGetQuery(string viewId,
            Dictionary<string, string> parameters,
            ResultFormat resultFormat)
        {
            var query = $"?viewId={Uri.EscapeDataString(viewId)}";
            query += $"&resultFormat={Uri.EscapeDataString(resultFormat.ToString())}";
            if(parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    query += $"&{parameter.Key}={parameter.Value}";
                }
            }
            return query;
        }

        public static async Task DeleteViewAsync(ApiConfiguration configuration, HttpClient httpClient, string viewId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/view/delete");
            requestUri += $"?viewId={Uri.EscapeDataString(viewId)}";
            var response = await httpClient.DeleteAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }
    }
}
