using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.Validation;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataCollectionProtocolParameter")]
    public interface IDataCollectionProtocolParameter : IId
    {
        [Required]
        string Name { get; }

        string DefaultValue { get; }

        [Required]
        bool IsMandatory { get; }

        [Required]
        DataCollectionProtocolParameterType Type { get; }

        [RequiredIfPropertyEqualTo(nameof(Type), DataCollectionProtocolParameterType.DataType)]
        string DataType { get; }
    }
}