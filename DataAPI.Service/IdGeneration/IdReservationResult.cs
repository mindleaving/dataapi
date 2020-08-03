namespace DataAPI.Service.IdGeneration
{
    public class IdReservationResult
    {
        private IdReservationResult(bool isReserved, string id, bool isNewCollection)
        {
            IsReserved = isReserved;
            Id = id;
            IsNewCollection = isNewCollection;
        }

        public static IdReservationResult Success(string id, bool isNewCollection)
        {
            return new IdReservationResult(true, id, isNewCollection);
        }

        public static IdReservationResult Failed()
        {
            return new IdReservationResult(false, null, false);
        }

        public bool IsReserved { get; }
        public string Id { get; }
        public bool IsNewCollection { get; }
    }
}