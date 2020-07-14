using Newtonsoft.Json;

namespace DataAPI.DataStructures.UserManagement
{
    public class AuthenticationResult
    {
        [JsonConstructor]
        private AuthenticationResult(bool isAuthenticated,
            AuthenticationErrorType error,
            string username,
            string accessToken)
        {
            IsAuthenticated = isAuthenticated;
            Error = error;
            AccessToken = accessToken;
            Username = username;
        }

        public static AuthenticationResult Success(string username, string accessToken)
        {
            return new AuthenticationResult(true, AuthenticationErrorType.Ok, username, accessToken);
        }

        public static AuthenticationResult Failed(AuthenticationErrorType errorType)
        {
            return new AuthenticationResult(false, errorType, null, null);
        }

        public bool IsAuthenticated { get; }
        public string Username { get; }
        public string AccessToken { get; }
        public AuthenticationErrorType Error { get; }
    }
}