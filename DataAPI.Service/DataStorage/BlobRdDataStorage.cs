using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons.Extensions;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.DomainModels;
using DataAPI.Service.Helpers;
using DataAPI.Service.IdGeneration;
using DataAPI.Service.Objects;
using MongoDB.Bson;

namespace DataAPI.Service.DataStorage
{
    public class BlobRdDataStorage : IBinaryRdDataStorage
    {
        private readonly IBinaryDataStorage binaryDataStorage;
        private readonly IRdDataStorage metadataStorage;
        private readonly IIdGeneratorManager idGeneratorManager;

        private readonly Dictionary<string, BinaryDataObjectSplitter> binaryDataObjectSplitters = new Dictionary<string, BinaryDataObjectSplitter>
        {
            {nameof(DataBlob), new BinaryDataObjectSplitter(nameof(DataBlob.Data))},
            {nameof(Image), new BinaryDataObjectSplitter(nameof(Image.Data))},
            {"RedirectedDataType", null } // For integration testing
        };
        private readonly string[] supportedDataTypes;

        public BlobRdDataStorage(
            string id,
            IBinaryDataStorage binaryDataStorage,
            IRdDataStorage metadataStorage,
            IIdGeneratorManager idGeneratorManager)
        {
            Id = id;
            this.binaryDataStorage = binaryDataStorage;
            this.metadataStorage = metadataStorage;
            this.idGeneratorManager = idGeneratorManager;
            supportedDataTypes = binaryDataObjectSplitters.Keys.ToArray();
        }

        public string Id { get; }

        public bool IsDataTypeSupported(string dataType)
        {
            return dataType.InSet(supportedDataTypes);
        }

        public bool IsIdGeneratorTypeSupported(IdGeneratorType idGeneratorType)
        {
            return true;
        }

        public bool IsValidId(string id)
        {
            // Azure blobs support any ID, so we are only restricted by the metadata store
            return metadataStorage.IsValidId(id);
        }

        public async Task<List<IdReservationResult>> GetIdsAsync(string dataType, string submitter, int count)
        {
            var ids = await idGeneratorManager.GetIdsAsync(dataType, count);
            var reservationResults = new List<IdReservationResult>();
            foreach (var id in ids)
            {
                var reservationResult = await ReserveIdAsync(dataType, id, submitter);
                reservationResults.Add(reservationResult);
            }
            return reservationResults;
        }

        public async Task<IdReservationResult> ReserveIdAsync(string dataType, string id, string submitter)
        {
            if (await ExistsAsync(dataType, id))
                return IdReservationResult.Failed();
            try
            {
                var isNewCollection = await IsNewCollection(dataType);
                var utcNow = DateTime.UtcNow;
                await metadataStorage.StoreAsync(
                    dataType,
                    new GenericDataContainer(id, submitter, utcNow, submitter, utcNow, ApiVersion.Current, new BsonDocument()),
                    false);
                var container = binaryDataStorage.GetContainer(dataType);
                await container.CreateIfNotExistsAsync();
                var blob = binaryDataStorage.GetBlob(dataType, id);
                await blob.WriteAsync(new MemoryStream(Encoding.ASCII.GetBytes("Reserved")));
                return IdReservationResult.Success(id, isNewCollection);
            }
            catch (DocumentAlreadyExistsException)
            {
                return IdReservationResult.Failed();
            }
        }

        public async Task<bool> ExistsAsync(string dataType, string id)
        {
            if (await metadataStorage.ExistsAsync(dataType, id))
                return true;
            var container = binaryDataStorage.GetContainer(dataType);
            if (!await container.ExistsAsync())
                return false;
            var blob = container.GetBlob(id);
            return await blob.ExistsAsync();
        }

        public async Task<StoreResult> StoreAsync(string dataType, GenericDataContainer container, bool overwrite)
        {
            var id = container.Id;
            var exists = await ExistsAsync(dataType, id);
            if (exists && !overwrite)
                throw new DocumentAlreadyExistsException($"Object of type '{dataType}' with ID '{id}' already exists");
            var isNewCollection = await IsNewCollection(dataType);
            var binaryDataObjectSplitter = binaryDataObjectSplitters[dataType];
            var splitResult = binaryDataObjectSplitter.Split(container);
            await metadataStorage.StoreAsync(dataType, splitResult.ContainerWithoutBinaryData, true);
            var blobContainer = binaryDataStorage.GetContainer(dataType);
            await blobContainer.CreateIfNotExistsAsync();
            var blob = blobContainer.GetBlob(id);
            await blob.WriteAsync(splitResult.BinaryData);
            var dataModificationType = exists
                ? DataModificationType.Replaced
                : DataModificationType.Created;
            return new StoreResult(id, dataModificationType, isNewCollection);
        }

        public async Task InjectDataAsync(string dataType, string id, Stream stream)
        {
            if (!await ExistsAsync(dataType, id))
                throw new SubmissionNotFoundException(dataType, id);
            var blob = binaryDataStorage.GetBlob(dataType, id);
            await blob.WriteAsync(stream);
        }

        public Task<GenericDataContainer> GetMetadataFromId(string dataType, string id)
        {
            return metadataStorage.GetFromIdAsync(dataType, id);
        }

        public async Task<Stream> GetBinaryDataFromIdAsync(string dataType, string id)
        {
            if (!await ExistsAsync(dataType, id))
                return null;
            var blob = binaryDataStorage.GetBlob(dataType, id);
            if (!await blob.ExistsAsync())
                return null;
            return blob.GetStream();
        }

        public async Task<GenericDataContainer> GetFromIdAsync(string dataType, string id)
        {
            if (!await ExistsAsync(dataType, id))
                return null;
            var blob = binaryDataStorage.GetBlob(dataType, id);
            await using var dataStream = blob.GetStream();
            var data = await ReadAllBytes(dataStream);
            var metadata = await metadataStorage.GetFromIdAsync(dataType, id)
                ?? await FixMissingMetadata(dataType, id, data);
            var binaryDataObjectSplitter = binaryDataObjectSplitters[dataType];
            return binaryDataObjectSplitter.Reassemble(metadata, data);
        }

        private async Task<GenericDataContainer> FixMissingMetadata(string dataType, string id, byte[] binaryData)
        {
            // Assumption: Metadata doesn't exist

            var blob = binaryDataStorage.GetBlob(dataType, id);
            DateTime createdTimeUtc;
            DateTime submissionTimeUtc;
            if (await blob.ExistsAsync())
            {
                createdTimeUtc = blob.CreatedTimestampUtc;
                submissionTimeUtc = blob.LastModifiedTimestampUtc;
            }
            else
            {
                var utcNow = DateTime.UtcNow;
                createdTimeUtc = utcNow;
                submissionTimeUtc = utcNow;
            }
            IId obj;
            switch (dataType)
            {
                case nameof(DataBlob):
                    obj = new DataBlob(id, new byte[0]);
                    break;
                case nameof(Image):
                    var imageFormat = ImageFormatDetector.Detect(binaryData);
                    var imageExtension = imageFormat != ImageFormatDetector.ImageFormat.Unknown ? imageFormat.GetFileExtension() : ".bin";
                    obj = new Image(id, new byte[0], imageExtension);
                    break;
                default:
                    throw new NotSupportedException($"Cannot fix missing metadata for ID '{id}' of data type '{dataType}'");
            }
            var container = new GenericDataContainer("unknown", createdTimeUtc, "unknown", submissionTimeUtc, ApiVersion.Current, obj);
            await metadataStorage.StoreAsync(dataType, container, false);

            return container;
        }

        public async IAsyncEnumerable<GenericDataContainer> GetManyAsync(
            string dataType,
            string whereArguments,
            string orderByArguments,
            uint? limit = null)
        {
            var metadata = metadataStorage.GetManyAsync(dataType, whereArguments, orderByArguments, limit);
            await foreach (var obj in metadata)
            {
                // PERFORMANCE: GetFromId re-requests metadata.
                // Either use a query that only retreives object-IDs above,
                // or get binary data and assemble full container here.
                var fullObj = await GetFromIdAsync(dataType, obj.Id);
                if(fullObj == null)
                    continue;
                yield return fullObj;
            }
        }

        public IAsyncEnumerable<BsonDocument> SearchAsync(DataApiSqlQuery parsedQuery, uint? maxResults = null)
        {
            return metadataStorage.SearchAsync(parsedQuery, maxResults);
        }

        public async Task<bool> DeleteDataContainerAsync(string dataType, string id)
        {
            if (await IsNewCollection(dataType))
                return true;
            var blob = binaryDataStorage.GetBlob(dataType, id);
            if (!await blob.ExistsAsync())
                return true;

            await metadataStorage.DeleteDataContainerAsync(dataType, id);
            try
            {
                await blob.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IAsyncEnumerable<string> ListCollectionNamesAsync()
        {
            var collectionNames = binaryDataStorage.ListContainers().ToList();
            return AsyncEnumerableBuilder.FromArray(collectionNames);
        }

        private async Task<bool> IsNewCollection(string dataType)
        {
            var container = binaryDataStorage.GetContainer(dataType);
            return !await container.ExistsAsync();
        }

        private static async Task<byte[]> ReadAllBytes(Stream stream)
        {
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
