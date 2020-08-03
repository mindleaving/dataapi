using DataAPI.DataStructures.DataSubscription;

namespace DataAPI.Service.Objects
{
    public class StoreResult
    {
        public StoreResult(string id, DataModificationType modificationType, bool isNewCollection)
        {
            Id = id;
            ModificationType = modificationType;
            IsNewCollection = isNewCollection;
        }

        public string Id { get; }
        public DataModificationType ModificationType { get; }
        public bool IsNewCollection { get; }
    }
}