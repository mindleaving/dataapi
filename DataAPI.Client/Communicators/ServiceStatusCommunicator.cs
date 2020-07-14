using System.Net.Http;

namespace DataAPI.Client.Communicators
{
    internal static class ServiceStatusCommunicator
    {
        public static bool IsAvailable(ApiConfiguration configuration, HttpClient httpClient)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/servicestatus/ping");
            try
            {
                var response = httpClient.GetAsync(requestUri).Result;
                ErrorReporter.EnsureSuccesStatusCode(response);
                var content = response.Content.ReadAsStringAsync().Result;
                return content.ToLowerInvariant() == "pong";
            }
            catch
            {
                return false;
            }
        }
    }
}
