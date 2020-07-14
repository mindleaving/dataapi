using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DataAPI.Client.Serialization;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.Exceptions;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;

namespace DataAPI.Client.Communicators
{
    internal static class AccountCommunicator
    {
        public static async Task<List<UserProfile>> GetAllUserProfiles(ApiConfiguration configuration, HttpClient httpClient)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/getuserprofiles");
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var stream = await response.Content.ReadAsStreamAsync();
            return await stream.ReadAllSearchResultsAsync<UserProfile>();
        }

        public static async Task<List<Role>> GetGlobalRolesForUserAsync(ApiConfiguration configuration, HttpClient httpClient, string username)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/getglobalroles");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            var roles = ConfiguredJsonSerializer.Deserialize<List<Role>>(json);
            return roles;
        }

        public static AuthenticationResult Login(ApiConfiguration configuration, HttpClient httpClient, string username, string password)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/login");

            var body = PostBodyBuilder.ConstructBody(new LoginInformation(username, password));
            var response = httpClient.PostAsync(requestUri, body).Result;
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Unauthorized)
            {
                var errorText = response.Content.ReadAsStringAsync().Result;
                throw new ApiException(response.StatusCode, errorText);
            }

            var json = response.Content.ReadAsStringAsync().Result;
            var authenticationResult = ConfiguredJsonSerializer.Deserialize<AuthenticationResult>(json);
            return authenticationResult;
        }

        public static AuthenticationResult LoginWithActiveDirectory(ApiConfiguration configuration, HttpClient httpClient)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/loginwithad");

            var response = httpClient.GetAsync(requestUri).Result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorText = response.Content.ReadAsStringAsync().Result;
                throw new ApiException(response.StatusCode, errorText);
            }

            var json = response.Content.ReadAsStringAsync().Result;
            var authenticationResult = ConfiguredJsonSerializer.Deserialize<AuthenticationResult>(json);
            return authenticationResult;
        }

        public static void Logout(ApiConfiguration configuration, HttpClient httpClient)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/logout");
            var response = httpClient.GetAsync(requestUri).Result;
            httpClient.DefaultRequestHeaders.Authorization = null;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void Register(ApiConfiguration configuration, HttpClient httpClient, string username, string firstName, string lastName, string password, string email)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/register");
            var body = PostBodyBuilder.ConstructBody(new RegistrationInformation(username, firstName, lastName, password, email));
            var response = httpClient.PostAsync(requestUri, body).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void ChangePassword(ApiConfiguration configuration, HttpClient httpClient, string username, string password)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/changepassword");
            var body = PostBodyBuilder.ConstructBody(new ChangePasswordBody(username, password));
            var response = httpClient.PostAsync(requestUri, body).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void AddGlobalRoleToUser(ApiConfiguration configuration, HttpClient httpClient, string username, Role role)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/addrole");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            requestUri += $"&role={Uri.EscapeDataString(role.ToString())}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void AddCollectionRoleToUser(ApiConfiguration configuration, HttpClient httpClient, string username, Role role, string dataType)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/addrole");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            requestUri += $"&role={Uri.EscapeDataString(role.ToString())}";
            requestUri += $"&dataType={Uri.EscapeDataString(dataType)}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void SetGlobalRolesForUser(ApiConfiguration configuration, HttpClient httpClient, string username, IList<Role> roles)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/setroles");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            var aggregatedRoles = roles.Select(role => role.ToString()).Aggregate((a, b) => a + "|" + b);
            requestUri += $"&roles={Uri.EscapeDataString(aggregatedRoles)}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void SetCollectionRoleForUser(ApiConfiguration configuration, HttpClient httpClient, string username, IList<Role> roles, string dataType)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/setroles");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            var aggregatedRoles = roles.Select(role => role.ToString()).Aggregate((a, b) => a + "|" + b);
            requestUri += $"&roles={Uri.EscapeDataString(aggregatedRoles)}";
            requestUri += $"&dataType={Uri.EscapeDataString(dataType)}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void RemoveGlobalRoleFromUser(ApiConfiguration configuration, HttpClient httpClient, string username, Role role)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/removerole");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            requestUri += $"&role={Uri.EscapeDataString(role.ToString())}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static void RemoveCollectionRoleFromUser(ApiConfiguration configuration, HttpClient httpClient, string username, Role role, string dataType)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/removerole");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            requestUri += $"&role={Uri.EscapeDataString(role.ToString())}";
            requestUri += $"&dataType={Uri.EscapeDataString(dataType)}";
            var response = httpClient.GetAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }

        public static async Task<List<CollectionUserPermissions>> GetCollectionPermissions(
            ApiConfiguration configuration, 
            HttpClient httpClient,
            string dataType)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/getcollectionpermissions");
            requestUri += $"?collectionName={Uri.EscapeDataString(dataType)}";
            var response = await httpClient.GetAsync(requestUri);
            ErrorReporter.EnsureSuccesStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return ConfiguredJsonSerializer.Deserialize<List<CollectionUserPermissions>>(json);
        }

        public static void DeleteUser(ApiConfiguration configuration, HttpClient httpClient, string username)
        {
            var requestUri = RequestUriBuilder.Build(configuration, "api/account/deleteuser");
            requestUri += $"?username={Uri.EscapeDataString(username)}";
            var response = httpClient.DeleteAsync(requestUri).Result;
            ErrorReporter.EnsureSuccesStatusCode(response);
        }
    }
}
