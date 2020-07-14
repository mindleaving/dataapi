using System;
using DataAPI.DataStructures.DomainModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.DataStorage
{
    public class GenericDataContainer
    {
        public GenericDataContainer() { }

        [JsonConstructor]
        public GenericDataContainer(
            string id,
            string originalSubmitter,
            DateTime? createdTimeUtc,
            string submitter,
            DateTime submissionTimeUtc,
            string apiVersion,
            BsonDocument data)
        {
            Id = id;
            OriginalSubmitter = originalSubmitter ?? submitter;
            CreatedTimeUtc = createdTimeUtc ?? submissionTimeUtc;
            Submitter = submitter;
            SubmissionTimeUtc = submissionTimeUtc;
            ApiVersion = apiVersion;
            Data = data;
        }

        public GenericDataContainer(
            string originalSubmitter,
            DateTime createdTimeUtc,
            string submitter,
            DateTime submissionTimeUtc,
            string apiVersion,
            IId obj)
            : this(obj.Id, originalSubmitter, createdTimeUtc, submitter, submissionTimeUtc, apiVersion, DataEncoder.Encode(obj))
        {
        }

        [BsonId]
        public string Id { get; private set; }

        public string OriginalSubmitter { get; private set; }
        public DateTime? CreatedTimeUtc { get; private set; }
        public string Submitter { get; private set; }
        public DateTime SubmissionTimeUtc { get; private set; }
        public string ApiVersion { get; private set; }

        [JsonConverter(typeof(BsonJsonConverter))]
        public BsonDocument Data { get; private set; }
    }

}
