namespace DataAPI.Service.IdGeneration
{
    public class SuggestedId
    {
        public SuggestedId(string id, bool hasBeenReserved)
        {
            Id = id;
            HasBeenReserved = hasBeenReserved;
        }

        public string Id { get; }
        public bool HasBeenReserved { get; }
    }
}