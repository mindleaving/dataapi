using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using MongoDB.Bson;

namespace DataAPI.Service.DataStorage
{
    public interface IRdDataStorage
    {
        string Id { get; }
        bool IsDataTypeSupported(string dataType);
        bool IsIdGeneratorTypeSupported(IdGeneratorType idGeneratorType);

        bool IsValidId(string id);
        Task<List<IdReservationResult>> GetIdsAsync(string dataType, string submitter, int count);
        Task<IdReservationResult> ReserveIdAsync(string dataType, string id, string submitter);
        Task<StoreResult> StoreAsync(string dataType, GenericDataContainer container, bool overwrite);
        Task<bool> ExistsAsync(string dataType, string id);
        Task<GenericDataContainer> GetFromIdAsync(string dataType, string id);
        IAsyncEnumerable<GenericDataContainer> GetManyAsync(
            string dataType,
            string whereArguments,
            string orderByArguments,
            uint? limit = null);
        IAsyncEnumerable<BsonDocument> SearchAsync(DataApiSqlQuery parsedQuery, uint? maxResults = null);
        Task<bool> DeleteDataContainerAsync(string dataType, string id);
        IAsyncEnumerable<string> ListCollectionNamesAsync();
    }

    public interface IBinaryRdDataStorage : IRdDataStorage
    {
        Task InjectDataAsync(string dataType, string id, Stream stream);
        Task<Stream> GetBinaryDataFromIdAsync(string dataType, string id);
        Task<GenericDataContainer> GetMetadataFromId(string dataType, string id);
    }
}