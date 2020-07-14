using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.PostBodies;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client.Communicators
{
    internal static class DataIoCommunicator
    {
        public static async Task<string> InsertAsync(ApiConfiguration configuration, HttpClient httpClient, object obj, string id = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/submit");
            var body = ConstructSubmitBodyFromObject(obj, false, id);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> InsertAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string json, string id = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/submit");
            var body = ConstructSubmitBodyFromJson(dataType, json, false, id);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> ReplaceAsync(ApiConfiguration configuration, HttpClient httpClient, object obj, string existingId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/submit");
            var body = ConstructSubmitBodyFromObject(obj, true, existingId);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> ReplaceAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string json, string existingId)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/submit");
            var body = ConstructSubmitBodyFromJson(dataType, json, true, existingId);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        private static HttpContent ConstructSubmitBodyFromObject(object obj, bool overwrite, string id)
        {
            var dataType = CollectionNameDeterminer.GetCollectionName(obj.GetType());
            var submitDataBody = new SubmitDataBody(dataType, obj, overwrite, id);
            return PostBodyBuilder.ConstructBody(submitDataBody);
        }

        private static HttpContent ConstructSubmitBodyFromJson(string dataType, string json, bool overwrite, string id)
        {
            var submitDataBody = new SubmitDataBody(dataType, null, overwrite, id);
            var submitBodyJsonObject = JObject.FromObject(submitDataBody);
            submitBodyJsonObject["Data"] = JObject.Parse(json);
            return PostBodyBuilder.ConstructBodyFromJObject(submitBodyJsonObject);
        }

        public static async Task<bool> ExistsAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/exists");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.GetAsync(requestUri);
            if (response.StatusCode == HttpStatusCode.OK)
                return true;
            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;
            ErrorReporter.EnsureSuccesStatusCode(response);
            throw new Exception("Didn't expect to get to this line of code");
        }

        public static async Task<T> GetAsync<T>(ApiConfiguration configuration, HttpClient httpClient, string id)
        {
            var json = await GetAsync(configuration, httpClient, DataApiClient.GetCollectionName<T>(), id);
            return !string.IsNullOrEmpty(json) ? ConfiguredJsonSerializer.Deserialize<T>(json) : default;
        }

        public static async Task<string> GetAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/get");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<List<T>> GetManyAsync<T>(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string whereArguments = null,
            string orderByArguments = null,
            uint? limit = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/getmany");
            var dataType = DataApiClient.GetCollectionName<T>();
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            if(whereArguments != null)
                requestUri += $"&whereArguments={Uri.EscapeDataString(whereArguments)}";
            if (orderByArguments != null)
                requestUri += $"&orderByArguments={Uri.EscapeDataString(orderByArguments)}";
            if(limit.HasValue)
                requestUri += $"&limit={limit}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            using (var stream = await response.Content.ReadAsStreamAsync())
                return await stream.ReadAllSearchResultsAsync<T>();
        }

        public static async Task<List<string>> GetManyAsync(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string dataType,
            string whereArguments,
            string orderByArguments = null,
            uint? limit = null)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/getmany");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            if(whereArguments != null)
                requestUri += $"&whereArguments={Uri.EscapeDataString(whereArguments)}";
            if (orderByArguments != null)
                requestUri += $"&orderByArguments={Uri.EscapeDataString(orderByArguments)}";
            if(limit.HasValue)
                requestUri += $"&limit={limit}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            using (var stream = await response.Content.ReadAsStreamAsync())
                return await stream.ReadAllLinesAsync();
        }

        public static async Task DeleteAsync(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/delete");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.DeleteAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<List<DeleteResult>> DeleteMany(ApiConfiguration configuration, HttpClient httpClient, string dataType, string whereArguments)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/deletemany");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&whereArguments={Uri.EscapeDataString(whereArguments)}";
            var response = await httpClient.DeleteAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            var deleteResults = ConfiguredJsonSerializer.Deserialize<List<DeleteResult>>(json);
            return deleteResults;
        }

        public static async Task<Stream> SearchAsync(ApiConfiguration configuration, HttpClient httpClient, string query, ResultFormat resultFormat)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/search");
            var body = ConstructSearchBodyFromObject(query, resultFormat);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStreamAsync();
        }

        private static HttpContent ConstructSearchBodyFromObject(string query, ResultFormat resultFormat)
        {
            var searchBody = new SearchBody(query, resultFormat);
            return PostBodyBuilder.ConstructBody(searchBody);
        }

        public static void SetDataRedirection(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string dataType, 
            string dataSourceSystem)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/setredirection");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&dataSourceSystem={Uri.EscapeDataString(dataSourceSystem)}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void SetCollectionOptions(
            ApiConfiguration configuration, HttpClient httpClient,
            CollectionOptions collectionOptions)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/setcollectionoptions");
            var body = PostBodyBuilder.ConstructBody(collectionOptions);
            var response = httpClient.PostAsync(requestUri, body).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<CollectionInformation> GetCollectionInformationAsync(ApiConfiguration configuration, HttpClient httpClient, string collectionName)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/getcollectioninformation");
            requestUri += $"?collectionName={Uri.EscapeDataString(collectionName)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return ConfiguredJsonSerializer.Deserialize<CollectionInformation>(json);
        }

        public static async Task<List<string>> ListCollectionNamesAsync(ApiConfiguration configuration, HttpClient httpClient, bool includeHidden)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/listcollectionnames");
            requestUri += $"?includeHidden={includeHidden}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            var searchResult = ConfiguredJsonSerializer.Deserialize<List<string>>(json);
            return searchResult;
        }

        public static async Task<List<CollectionInformation>> ListCollectionsAsync(ApiConfiguration configuration, HttpClient httpClient, bool includeHidden)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/listcollections");
            requestUri += $"?includeHidden={includeHidden}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            var searchResult = ConfiguredJsonSerializer.Deserialize<List<CollectionInformation>>(json);
            return searchResult;
        }

        public static async Task<string> CreateSubmission<T>(ApiConfiguration configuration, HttpClient httpClient, T obj, Func<T, byte[]> binaryDataPath, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/createsubmission");
            var body = ConstructSubmitBodyFromObject(obj, false, id);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> CreateSubmission(ApiConfiguration configuration, HttpClient httpClient, JObject obj, string binaryDataPath, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/createsubmission");
            var clonedJObject = obj.DeepClone();
            clonedJObject[binaryDataPath] = null;
            var body = ConstructSubmitBodyFromJson(dataType, clonedJObject.ToString(), false, id);
            var response = await httpClient.PostAsync(requestUri, body);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task TransferSubmissionData<T>(ApiConfiguration configuration, HttpClient httpClient, T obj, Func<T, byte[]> binaryDataPath, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/transfersubmissiondata");
            var dataType = DataApiClient.GetCollectionName<T>();
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&submissionId={Uri.EscapeDataString(id)}";
            using (var byteStream = new MemoryStream(binaryDataPath(obj)))
                await TransferSubmissionData(requestUri, byteStream, httpClient);
        }

        public static async Task TransferSubmissionData(ApiConfiguration configuration, HttpClient httpClient, JObject jObject, string binaryDataPath, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/transfersubmissiondata");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&submissionId={Uri.EscapeDataString(id)}";
            var bytes = jObject[binaryDataPath].Value<byte[]>();
            using (var byteStream = new MemoryStream(bytes))
                await TransferSubmissionData(requestUri, byteStream, httpClient);
        }

        public static async Task TransferSubmissionData(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id, Stream stream)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/transfersubmissiondata");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&submissionId={Uri.EscapeDataString(id)}";
            await TransferSubmissionData(requestUri, stream, httpClient);
        }

        private static async Task TransferSubmissionData(string requestUri, Stream stream, HttpClient httpClient)
        {
            var response = await httpClient.PostAsync(requestUri, new StreamContent(stream));
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<T> GetSubmissionMetadata<T>(ApiConfiguration configuration, HttpClient httpClient, string id)
        {
            var json = await GetSubmissionMetadata(configuration, httpClient, DataApiClient.GetCollectionName<T>(), id);
            if (string.IsNullOrEmpty(json))
                return default(T);
            return ConfiguredJsonSerializer.Deserialize<T>(json);
        }

        public static async Task<string> GetSubmissionMetadata(ApiConfiguration configuration, HttpClient httpClient, string dataType, string id)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/dataio/getsubmissionmetadata");
            requestUri += $"?dataType={Uri.EscapeDataString(dataType)}";
            requestUri += $"&id={Uri.EscapeDataString(id)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<T> FirstOrDefault<T>(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string whereArguments,
            string orderByArguments)
        {
            return (await GetManyAsync<T>(configuration, httpClient, whereArguments, orderByArguments, 1)).FirstOrDefault();
        }

        public static async Task<string> FirstOrDefault(
            ApiConfiguration configuration,
            HttpClient httpClient,
            string dataType,
            string whereArguments,
            string orderByArguments)
        {
            return (await GetManyAsync(configuration, httpClient, dataType, whereArguments, orderByArguments, 1)).FirstOrDefault();
        }
    }
}
