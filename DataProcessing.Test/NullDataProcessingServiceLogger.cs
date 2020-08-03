using System.Threading.Tasks;
using DataProcessing.Logging;

namespace DataProcessing.Test
{
    public class NullDataProcessingServiceLogger : IDataProcessingServiceLogger
    {
        public Task Log(DataProcessingServiceLog logEntry)
        {
            return Task.CompletedTask;
        }
    }
}