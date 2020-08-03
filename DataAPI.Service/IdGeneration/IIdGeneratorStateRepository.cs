using System;
using System.Threading.Tasks;

namespace DataAPI.Service.IdGeneration
{
    public interface IIdGeneratorStateRepository
    {
        Task<bool> TryGetLockAsync(IdGeneratorState idGeneratorState);
        Task<IdGeneratorState> GetForDataTypeAsync(string dataType);
        Task ReleaseLockAsync(IdGeneratorState idGeneratorState, Guid sessionId);
    }
}
