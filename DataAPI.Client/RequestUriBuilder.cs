namespace DataAPI.Client
{
    public static class RequestUriBuilder
    {
        /// <summary>
        /// Build request URI
        /// </summary>
        /// <param name="configuration">API configuration with server address and port</param>
        /// <param name="resourceName">Route to service, e.g. api/dataio/get (NO prefixed '/')</param>
        public static string Build(ApiConfiguration configuration, string resourceName)
        {
            const string Protocol = "https://";
            return $"{Protocol}{configuration.ServerAddress}:{configuration.ServerPort}/{resourceName}";
        }
    }
}
