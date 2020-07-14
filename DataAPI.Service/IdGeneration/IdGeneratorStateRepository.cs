using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace DataAPI.Service.IdGeneration
{
    public class IdGeneratorStateRepository : IIdGeneratorStateRepository
    {
        private readonly IMongoCollection<IdGeneratorState> idGeneratorStateCollection;

        public IdGeneratorStateRepository(IMongoCollection<IdGeneratorState> idGeneratorStateCollection)
        {
            this.idGeneratorStateCollection = idGeneratorStateCollection;
        }

        public async Task<bool> TryGetLockAsync(IdGeneratorState idGeneratorState)
        {
            if(!idGeneratorState.IsLocked)
                throw new Exception("Cannot get lock for state that is not locked");
            var replaceResult = await idGeneratorStateCollection.ReplaceOneAsync(
                x => x.DataType == idGeneratorState.DataType && !x.IsLocked,
                idGeneratorState,
                new UpdateOptions { IsUpsert = true });
            return replaceResult.IsAcknowledged && replaceResult.ModifiedCount == 1;
        }

        public Task<IdGeneratorState> GetForDataTypeAsync(string dataType)
        {
            return idGeneratorStateCollection.Find(x => x.DataType == dataType).FirstOrDefaultAsync();
        }

        public Task ReleaseLockAsync(IdGeneratorState idGeneratorState, Guid sessionId)
        {
            if(idGeneratorState.IsLocked)
                throw new Exception("Cannot release lock for state that is still locked");
            return idGeneratorStateCollection.ReplaceOneAsync(
                x => x.DataType == idGeneratorState.DataType && x.LockSessionId == sessionId,
                idGeneratorState);
        }
    }
}