﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataProject : IDataProject<DataCollectionProtocol, DataCollectionProtocolParameter, DataPlaceholder>
    {
        [JsonConstructor]
        public DataProject(
            string id, 
            string idSourceSystem,
            DataCollectionProtocol protocol,
            Dictionary<string, string> parameterResponse)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            IdSourceSystem = idSourceSystem;
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            ParameterResponse = parameterResponse ?? new Dictionary<string, string>();
        }

        [Required]
        public string Id { get; private set; }
        [Required]
        public string IdSourceSystem { get; private set; }
        [Required]
        public DataCollectionProtocol Protocol { get; private set; }
        public Dictionary<string, string> ParameterResponse { get; private set; }
    }
}
