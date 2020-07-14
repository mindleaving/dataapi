using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataAPI.Service.IdGeneration
{
    public interface IIdGenerator
    {
        Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default);
    }
}