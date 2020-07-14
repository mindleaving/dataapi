using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataAPI.Service.IdGeneration
{
    public interface IIdGeneratorManager
    {
        Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default);
        Task<IIdGenerator> GetIdGeneratorForDataTypeAsync(string dataType);
    }
}