using System;
using DataAPI.Client;
using NUnit.Framework;

namespace DataAPI.Tools
{
    public abstract class DataApiAccess
    {
        public static readonly ApiConfiguration ApiConfiguration = new ApiConfiguration("", 443);
        protected readonly IDataApiClient dataApiClient = new DataApiClient(ApiConfiguration);

        [OneTimeSetUp]
        public void Login()
        {
            if (!dataApiClient.IsAvailable())
                throw new Exception("API not available");
            dataApiClient.Login();
        }

        [OneTimeTearDown]
        public void Logout()
        {
            dataApiClient.Logout();
        }
    }
}
