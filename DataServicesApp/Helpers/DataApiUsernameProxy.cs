using DataAPI.Client;

namespace DataServicesApp.Helpers
{
    public class DataApiUsernameProxy : IUsernameProxy
    {
        private readonly IDataApiClient dataApiClient;

        public DataApiUsernameProxy(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
        }

        public string Username => dataApiClient.LoggedInUsername;
    }
}