using System.Threading.Tasks;

namespace DataAPI.Service.DataStorage
{
    public interface IBlobContainer
    {
        Task<bool> ExistsAsync();
        Task CreateIfNotExistsAsync();
        IBlob GetBlob(string id);
    }
}