using System.Threading.Tasks;
using DataAPI.Client;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Exceptions;

namespace DataServicesApp.Workflow
{
    public class SqlExpressionValidator : ISqlExpressionValidator
    {
        private readonly IDataApiClient dataApiClient;

        public SqlExpressionValidator(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
        }

        public async Task<(bool,string)> ValidateWhereAsync(string dataType, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return (true,null);
            var testQuery = $"SELECT * FROM {dataType} WHERE {filter} LIMIT 1";
            try
            {
                await dataApiClient.SearchAsync(testQuery, ResultFormat.Json);
                return (true, null);
            }
            catch (ApiException apiException)
            {
                return (false, apiException.Message);
            }
        }
    }
}