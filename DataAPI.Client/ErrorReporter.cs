using System.Net.Http;
using DataAPI.DataStructures.Exceptions;

namespace DataAPI.Client
{
    internal static class ErrorReporter
    {
        public static void EnsureSuccesStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorText = response.Content.ReadAsStringAsync().Result;
                throw new ApiException(response.StatusCode, !string.IsNullOrEmpty(errorText) ? errorText : response.ReasonPhrase);
            }
        }
    }
}