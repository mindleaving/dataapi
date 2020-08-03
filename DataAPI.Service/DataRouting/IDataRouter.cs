using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;

namespace DataAPI.Service.DataRouting
{
    public interface IDataRouter
    {
        bool IsAvailable(string dataSourceSystemId);
        Task<IRdDataStorage> GetSourceSystemAsync(string dataType);
        Task SetRedirectionAsync(DataRedirection dataRedirection);
        IAsyncEnumerable<string> ListCollectionNamesAsync();
        bool IsDataTypeSupported(string dataType, string dataSourceSystemId);
    }
}