using System;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace DataAPI.Service.DataStorage
{
    public class BinaryDataObjectSplitter
    {
        private readonly string binaryDataPath;

        public BinaryDataObjectSplitter(string binaryDataPath)
        {
            this.binaryDataPath = binaryDataPath;
        }

        public BinaryDataObjectSplitterResult Split(GenericDataContainer container)
        {
            var payload = container.Data.DeepClone().AsBsonDocument;
            var bsonValue = payload.GetValue(binaryDataPath);
            var base64 = bsonValue.IsString ? bsonValue.AsString : null;
            var binaryData = base64 != null ? Convert.FromBase64String(base64) : new byte[0];
            payload.Set(binaryDataPath, BsonString.Empty);
            var containerWithoutData =  new GenericDataContainer(
                container.Id,
                container.OriginalSubmitter,
                container.CreatedTimeUtc,
                container.Submitter,
                container.SubmissionTimeUtc,
                container.ApiVersion,
                payload);
            return new BinaryDataObjectSplitterResult(containerWithoutData, binaryData);
        }

        public GenericDataContainer Reassemble(GenericDataContainer containerWithoutBinaryData, byte[] binaryData)
        {
            var containerWithData = CloneGenericDataContainer(containerWithoutBinaryData);
            containerWithData.Data.Set(binaryDataPath, BsonString.Create(Convert.ToBase64String(binaryData)));
            return containerWithData;
        }

        private static GenericDataContainer CloneGenericDataContainer(GenericDataContainer containerWithoutBinaryData)
        {
            var json = JsonConvert.SerializeObject(containerWithoutBinaryData);
            return JsonConvert.DeserializeObject<GenericDataContainer>(json);
        }
    }

    public class BinaryDataObjectSplitterResult
    {
        public BinaryDataObjectSplitterResult(
            GenericDataContainer containerWithoutBinaryData, 
            byte[] binaryData)
        {
            ContainerWithoutBinaryData = containerWithoutBinaryData ?? throw new ArgumentNullException(nameof(containerWithoutBinaryData));
            BinaryData = binaryData ?? throw new ArgumentNullException(nameof(binaryData));
        }

        public GenericDataContainer ContainerWithoutBinaryData { get; }
        public byte[] BinaryData { get; }
    }
}
