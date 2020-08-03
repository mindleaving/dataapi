using System.Threading.Tasks;

namespace DataProcessing.Logging
{
    public interface IDataProcessingServiceLogger
    {
        Task Log(DataProcessingServiceLog logEntry);
    }
}