using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataCollectionProtocol : IDataCollectionProtocol<DataCollectionProtocolParameter, DataPlaceholder>
    {
        [JsonConstructor]
        public DataCollectionProtocol(
            string id, 
            List<DataCollectionProtocolParameter> parameters, 
            List<DataPlaceholder> expectedData)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Parameters = parameters ?? new List<DataCollectionProtocolParameter>();
            ExpectedData = expectedData ?? new List<DataPlaceholder>();
        }

        [Required]
        public string Id { get; private set; }
        public List<DataCollectionProtocolParameter> Parameters { get; private set; }
        public List<DataPlaceholder> ExpectedData { get; private set; }
    }
}
