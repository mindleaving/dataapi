using DataAPI.Client;

namespace DataProcessing.Test
{
    public abstract class ApiTestBase
    {
        protected ApiTestBase()
        {
            DataApiClient = new DataApiClient(ApiSetup.ApiConfiguration);
            DataApiClient.Login(ApiSetup.UnitTestAdminUsername, ApiSetup.UnitTestAdminPassword);
        }

        protected IDataApiClient DataApiClient { get; }
    }
}