using System;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;
using DataAPI.Client.Communicators;
using DataAPI.DataStructures.UserManagement;
using Newtonsoft.Json.Linq;

namespace DataAPI.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Helper-class for auto-renewal of access tokens.
    /// Usage:
    /// - Provide valid API configuration (in constructor) and login information (by setting property <see cref="LoginInformation" />)
    /// - Call <see cref="Login" /> and make sure it returns <value>true</value>
    /// </summary>
    internal class LoginManager : IDisposable
    {
        private readonly ApiConfiguration apiConfiguration;
        private readonly IHttpClientProxy httpClientProxy;
        private AuthenticationResult latestAuthenticationResult;
        private readonly Timer loginRefreshTimer = new Timer();

        public LoginManager(ApiConfiguration apiConfiguration, IHttpClientProxy httpClientProxy)
        {
            this.apiConfiguration = apiConfiguration;
            this.httpClientProxy = httpClientProxy;
            loginRefreshTimer.Elapsed += LoginRefreshTimer_Elapsed;

            loginMethod = LoginMethod.ActiveDirectory;
            httpClientProxy.UseActiveDirectoryAuthorization = loginMethod == LoginMethod.ActiveDirectory;
        }

        public LoginInformation LoginInformation { get; set; }

        public TimeSpan RenewalPeriod { get; set; } = TimeSpan.FromMinutes(57);

        public bool IsLoggedIn => latestAuthenticationResult?.IsAuthenticated ?? false;
        public string LoggedInUsername => latestAuthenticationResult?.Username;
        private LoginMethod loginMethod;
        public LoginMethod LoginMethod
        {
            get => loginMethod;
            set
            {
                if(value == loginMethod)
                    return;
                Logout();
                loginMethod = value;
                httpClientProxy.UseActiveDirectoryAuthorization = loginMethod == LoginMethod.ActiveDirectory;
            }
        }

        public AuthenticationResult Login(bool force = false)
        {
            switch (loginMethod)
            {
                case LoginMethod.ActiveDirectory:
                    return LoginActiveDirectory();
                case LoginMethod.JsonWebToken:
                    if(LoginInformation == null)
                        throw new Exception("No login information provided");
                    return LoginJwt(force);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetAccessToken(string accessToken)
        {
            LoginMethod = LoginMethod.JsonWebToken;
            var claim = ParseJwt(accessToken);
            //if (claim.ContainsKey("exp"))
            //{
            //    var expirationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claim["exp"].Value<int>());
            //    if(expirationTime < DateTime.UtcNow)
            //        throw new InvalidOperationException("Access token is already expired");
            //}
            var username = ExtractUsernameFromJwt(claim);
            latestAuthenticationResult = AuthenticationResult.Success(username, accessToken);
            httpClientProxy.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private string ExtractUsernameFromJwt(JObject claim)
        {
            if(!claim.ContainsKey("unique_name"))
                throw new FormatException("JWT doesn't contain 'unique_name'-claim");
            return claim["unique_name"].Value<string>();
        }

        private static JObject ParseJwt(string accessToken)
        {
            var dotSplit = accessToken.Split('.');
            if (dotSplit.Length < 3)
                throw new FormatException("Access token appears not to be a valid JWT");
            var claimBase64 = PadBase64(dotSplit[1]);
            var base64Decoded = Encoding.UTF8.GetString(Convert.FromBase64String(claimBase64));
            var claim = JObject.Parse(base64Decoded);
            return claim;
        }

        private static string PadBase64(string unpadded)
        {
            Math.DivRem(unpadded.Length, 4, out var remainder);
            var padding = remainder == 0 ? 0 : 4 - remainder;
            return unpadded + new string('=', padding);
        }

        private AuthenticationResult LoginActiveDirectory()
        {
            latestAuthenticationResult = AccountCommunicator.LoginWithActiveDirectory(apiConfiguration, httpClientProxy.Client);
            return latestAuthenticationResult;
        }

        private AuthenticationResult LoginJwt(bool force)
        {
            if (IsLoggedIn && !force)
                return latestAuthenticationResult;
            TryRenewAccessToken();
            return latestAuthenticationResult;
        }

        private void LoginRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!TryRenewAccessToken())
            {
                Logout();
                throw new Exception("Could not renew access token");
            }
        }

        private bool TryRenewAccessToken()
        {
            try
            {
                latestAuthenticationResult = AccountCommunicator.Login(
                    apiConfiguration,
                    httpClientProxy.Client,
                    LoginInformation.Username,
                    LoginInformation.Password);
            }
            catch (Exception)
            {
                latestAuthenticationResult = AuthenticationResult.Failed(AuthenticationErrorType.AuthenticationMethodNotAvailable);
                return false;
            }

            if (!latestAuthenticationResult.IsAuthenticated)
                return false;

            SetAccessToken(latestAuthenticationResult.AccessToken);
            loginRefreshTimer.Interval = RenewalPeriod.TotalMilliseconds;
            if(!loginRefreshTimer.Enabled)
                loginRefreshTimer.Start();
            return true;
        }

        public void Logout()
        {
            if(!IsLoggedIn)
                return;
            loginRefreshTimer.Stop();
            AccountCommunicator.Logout(apiConfiguration, httpClientProxy.Client);
            httpClientProxy.Client.DefaultRequestHeaders.Authorization = null;
            latestAuthenticationResult = null;
        }

        public void Dispose()
        {
            Logout();
        }
    }
}

