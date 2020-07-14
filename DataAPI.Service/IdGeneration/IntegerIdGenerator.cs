using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAPI.Service.DataStorage;

namespace DataAPI.Service.IdGeneration
{
    public class IntegerIdGenerator : IIdGenerator
    {
        private readonly IIdGeneratorStateRepository idGeneratorStateRepository;

        public IntegerIdGenerator(RdDataMongoClient mongoClient)
        {
            var idGeneratorStateCollection = mongoClient.BackendDatabase.GetCollection<IdGeneratorState>(nameof(IdGeneratorState));
            idGeneratorStateRepository = new IdGeneratorStateRepository(idGeneratorStateCollection);
        }

        public TimeSpan MaxTryTime { get; set; } = 2 * IdGeneratorState.MaxLockTime;
        public TimeSpan TryPeriod { get; set; } = TimeSpan.FromSeconds(2);

        public IntegerIdGenerator(IIdGeneratorStateRepository idGeneratorStateRepository)
        {
            this.idGeneratorStateRepository = idGeneratorStateRepository;
        }

        public async Task<List<string>> GetIdsAsync(string dataType, int count, CancellationToken cancellationToken = default)
        {
            var idGeneratorState = await GetLockedStateAsync(dataType, cancellationToken);
            if(idGeneratorState == null)
                throw new TimeoutException("Could not obtain ID-lock. ID generation failed.");
            var startId = idGeneratorState.LastId + 1;
            idGeneratorState.LastId += count;
            await ReleaseLockAsync(idGeneratorState);
            return Enumerable.Range(startId, count).Select(x => x.ToString()).ToList();
        }

        private async Task<IdGeneratorState> GetLockedStateAsync(string dataType, CancellationToken cancellationToken)
        {
            var tryTimeStart = DateTime.UtcNow;
            do
            {
                var idGeneratorState = await idGeneratorStateRepository.GetForDataTypeAsync(dataType) ?? new IdGeneratorState(dataType, 0);
                if (idGeneratorState.IsLocked && DateTime.UtcNow - idGeneratorState.LockTime > IdGeneratorState.MaxLockTime)
                    await ReleaseLockAsync(idGeneratorState);
                if (!idGeneratorState.IsLocked)
                {
                    idGeneratorState.Lock();
                    if(await idGeneratorStateRepository.TryGetLockAsync(idGeneratorState))
                        return idGeneratorState;
                }
                await Task.Delay(TryPeriod, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            } while (DateTime.UtcNow - tryTimeStart < MaxTryTime);

            return null;
        }

        private async Task ReleaseLockAsync(IdGeneratorState idGeneratorState)
        {
            if(!idGeneratorState.IsLocked)
                return;
            var sessionId = idGeneratorState.LockSessionId.Value;
            idGeneratorState.Unlock(sessionId);
            await idGeneratorStateRepository.ReleaseLockAsync(idGeneratorState, sessionId);
        }
    }
}
