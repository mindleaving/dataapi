using System;
using System.Collections.Generic;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using Newtonsoft.Json;

namespace DataProcessing.Objects
{
    public class PostponedProcessingObject : IId
    {
        [JsonConstructor]
        public PostponedProcessingObject(
            string id,
            string processorName,
            DataModificationType modificationType,
            string dataType,
            string dataId,
            List<DataReference> missingData,
            TimeSpan maxWaitTime,
            int remainingAttempts,
            DateTime lastAttempt)
        {
            Id = id;
            ProcessorName = processorName;
            DataType = dataType;
            DataId = dataId;
            MissingData = missingData;
            MaxWaitTime = maxWaitTime;
            RemainingAttempts = remainingAttempts;
            LastAttempt = lastAttempt;
            ModificationType = modificationType;
        }

        public PostponedProcessingObject(
            string processorName,
            DataModificationType modificationType,
            string dataType,
            string dataId,
            List<DataReference> missingData,
            TimeSpan maxWaitTime,
            int remainingAttempts,
            DateTime lastAttempt)
        : this(
            IdGenerator.FromGuid(),
            processorName,
            modificationType,
            dataType, 
            dataId,
            missingData,
            maxWaitTime,
            remainingAttempts,
            lastAttempt)
        {
        }

        public string Id { get; }

        public string ProcessorName { get; }
        public DataModificationType ModificationType { get; }
        public string DataType { get; }
        public string DataId { get; }
        public List<DataReference> MissingData { get; }
        public TimeSpan MaxWaitTime { get; }
        public int RemainingAttempts { get; set; }
        public DateTime LastAttempt { get; set; }
    }
}
