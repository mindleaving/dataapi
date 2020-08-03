using Commons.Misc;
using DataAPI.Client;

namespace DataAPI.IntegrationTest
{
    public static class ApiSetup
    {
        public static string ServerAddress { get; } = "";
        public static ushort ServerPort { get; } = 443;
        public static ApiConfiguration ApiConfiguration { get; } = new ApiConfiguration(ServerAddress, ServerPort);

        public static string UnitTestAdminUsername { get; } = "UnitTestAdmin";
        public static string UnitTestAdminPassword { get; } = Secrets.Get("DataAPI_UnitTestCredentials");
    }
}
