using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.Validation;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Communicators
{
    internal static class ValidatorCommunicator
    {
        public static async Task SubmitValidatorAsync(
            ApiConfiguration configuration,
            HttpClient httpClient,
            ValidatorDefinition validatorDefinition,
            bool suppressAutoApprove)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/submit");
            var body = ConstructSubmitValidatorBody(validatorDefinition, suppressAutoApprove);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        private static HttpContent ConstructSubmitValidatorBody(ValidatorDefinition validatorDefinition, bool suppressAutoApprove)
        {
            var submitValidatorBody = new SubmitValidatorBody(validatorDefinition) {SuppressAutoApprove = suppressAutoApprove};
            return PostBodyBuilder.ConstructBody(submitValidatorBody);
        }

        public static async Task ApplyValidatorAsync<T>(
            ApiConfiguration configuration,
            HttpClient httpClient,
            T obj,
            string validatorId = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/apply");
            var body = ConstructApplyValidatorBodyFromObject(obj, validatorId);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        private static HttpContent ConstructApplyValidatorBodyFromObject(object obj, string validatorId)
        {
            var dataType = CollectionNameDeterminer.GetCollectionName(obj.GetType());
            var submitDataBody = new ApplyValidatorBody(dataType, obj, validatorId);
            return PostBodyBuilder.ConstructBody(submitDataBody);
        }

        public static async Task ApplyValidatorAsync(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string dataType,
            string json,
            string validatorId = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/apply");
            var body = ConstructApplyValidatorBodyFromJson(dataType, json, validatorId);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        private static HttpContent ConstructApplyValidatorBodyFromJson(string dataType, string json, string validatorId)
        {
            var submitDataBody = new ApplyValidatorBody(dataType, null, validatorId);
            var submitBodyJsonObject = JObject.FromObject(submitDataBody);
            submitBodyJsonObject["Data"] = JObject.Parse(json);
            return PostBodyBuilder.ConstructBodyFromJObject(submitBodyJsonObject);
        }

        public static async Task<ValidatorDefinition> GetValidatorDefinitionAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string validatorId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/get");
            requestUri += $"?validatorId={Uri.EscapeDataString(validatorId)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return ConfiguredJsonSerializer.Deserialize<ValidatorDefinition>(json);
        }

        public static async Task ApproveValidatorAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string validatorId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/approve");
            requestUri += $"?validatorId={Uri.EscapeDataString(validatorId)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task UnapproveValidatorAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string validatorId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/unapprove");
            requestUri += $"?validatorId={Uri.EscapeDataString(validatorId)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task DeleteValidatorAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string validatorId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/delete");
            requestUri += $"?validatorId={Uri.EscapeDataString(validatorId)}";
            var response = await httpClient.DeleteAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<List<ValidatorDefinition>> GetAllValidatorDefinitionsAsync(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string dataType = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/validator/getall");
            if (!string.IsNullOrEmpty(dataType))
                requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return ConfiguredJsonSerializer.Deserialize<List<ValidatorDefinition>>(json);
        }
    }
}
