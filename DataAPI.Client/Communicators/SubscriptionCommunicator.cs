using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;

namespace DataAPI.Client.Communicators
{
    internal static class SubscriptionCommunicator
    {
        public static async Task<string> SubscribeAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string dataType,
            IList<DataModificationType> modificationTypes,
            string filter = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/subscribe");
            var body = ConstructSubscriptionBody(dataType, modificationTypes, filter);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var subscriptionId = await response.Content.ReadAsStringAsync();
            return subscriptionId;
        }

        private static HttpContent ConstructSubscriptionBody(string dataType, IList<DataModificationType> modificationTypes, string filter)
        {
            var subscriptionBody = new SubscriptionBody(dataType, modificationTypes.ToList(), filter);
            return PostBodyBuilder.ConstructBody(subscriptionBody);
        }

        public static async Task UnsubscribeAsync(ApiConfiguration configuration, HttpClient httpClient, string subscriptionId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/unsubscribe");
            requestUri += $"?id={Uri.EscapeDataString(subscriptionId)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task UnsubscribeAllAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/unsubscribeall");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<List<SubscriptionInfo>> GetSubscriptionsAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/getsubscriptions");
            if(!string.IsNullOrEmpty(dataType))
                requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var stream = await response.Content.ReadAsStreamAsync();
            return await stream.ReadAllSearchResultsAsync<SubscriptionInfo>();
        }

        public static async Task<List<SubscriptionNotification>> GetSubscribedObjects(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string dataType = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/getsubscribedobjects");
            if(!string.IsNullOrEmpty(dataType))
                requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var stream = await response.Content.ReadAsStreamAsync();
            return await stream.ReadAllSearchResultsAsync<SubscriptionNotification>();
        }

        public static async Task DeleteNotificationAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient, 
            string notificationId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/deletenotification");
            requestUri += $"?notificationId={Uri.EscapeDataString(notificationId)}";
            var response = await httpClient.DeleteAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task ReportTo(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string recipient,
            string dataType,
            string dataObjectId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/subscription/reportto");
            requestUri += $"?recipient={Uri.EscapeDataString(recipient)}";
            requestUri += $"&dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(dataObjectId)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }
    }
}
