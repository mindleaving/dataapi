using System.Threading.Tasks;
using DataAPI.Client;

namespace DataProcessing.Logging
{
    public class DataProcessingServiceLogger : IDataProcessingServiceLogger
    {
        private readonly IDataApiClient dataApiClient;
        

        public DataProcessingServiceLogger(IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            
        }

        public async Task Log(DataProcessingServiceLog logEntry)
        {
            await dataApiClient.InsertAsync(logEntry, logEntry.Id);
        }
    }
}
