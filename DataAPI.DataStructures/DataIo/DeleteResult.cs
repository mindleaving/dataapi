using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataIo
{
    public class DeleteResult
    {
        [JsonConstructor]
        private DeleteResult(
            string dataType,
            string id,
            bool isDeleted,
            string errorMessage)
        {
            DataType = dataType;
            Id = id;
            IsDeleted = isDeleted;
            ErrorMessage = errorMessage;
        }

        public static DeleteResult Success(string dataType, string id)
        {
            return new DeleteResult(dataType, id, true, null);
        }

        public static DeleteResult Failed(string dataType, string id, string errorMessage)
        {
            return new DeleteResult(dataType, id, false, errorMessage);
        }

        public string DataType { get; }
        public string Id { get; }
        public bool IsDeleted { get; }
        public string ErrorMessage { get; }
    }
}
