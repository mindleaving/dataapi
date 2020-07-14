using System;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Constants;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataCollectionProtocolParameter
    {
        [JsonConstructor]
        public DataCollectionProtocolParameter(
            string name, 
            string defaultValue, 
            bool isMandatory,
            DataCollectionProtocolParameterType type,
            string dataType = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            DefaultValue = defaultValue;
            IsMandatory = isMandatory;
            Type = type;
            if(type == DataCollectionProtocolParameterType.DataType)
                DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        }

        [Required]
        public string Id => Name;
        [Required]
        public string Name { get; private set; }
        public string DefaultValue { get; private set; }
        [Required]
        public bool IsMandatory { get; private set; }
        [Required]
        public DataCollectionProtocolParameterType Type { get; private set; }
        public string DataType { get; private set; }
    }
}