using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace DataAPI.Service.IdGeneration
{
    public class IdGeneratorState
    {
        public static readonly TimeSpan MaxLockTime = TimeSpan.FromSeconds(10);

        [JsonConstructor]
        public IdGeneratorState(
            string dataType,
            int lastId,
            bool isLocked,
            DateTime? lockTime,
            Guid? lockSessionId)
        {
            DataType = dataType;
            LastId = lastId;
            IsLocked = isLocked;
            LockTime = lockTime;
            LockSessionId = lockSessionId;
        }

        public IdGeneratorState(string dataType, int lastId)
        {
            DataType = dataType;
            LastId = lastId;
        }

        [BsonId]
        public string DataType { get; private set; }
        public int LastId { get; set; }
        public bool IsLocked { get; private set; }
        public DateTime? LockTime { get; private set; }
        public Guid? LockSessionId { get; private set; } 

        public Guid Lock()
        {
            if (IsLocked && DateTime.UtcNow - LockTime.Value < TimeSpan.FromSeconds(30))
                throw new InvalidOperationException($"ID generator state for data type '{DataType}' is already locked");
            IsLocked = true;
            LockTime = DateTime.UtcNow;
            LockSessionId = Guid.NewGuid();
            return LockSessionId.Value;
        }

        public void Unlock(Guid sessionId)
        {
            if(!IsLocked)
                return;
            if(LockSessionId != sessionId)
                throw new InvalidOperationException("Cannot unlock session that isn't your own'");
            IsLocked = false;
            LockSessionId = null;
            LockTime = null;
        }
    }
}