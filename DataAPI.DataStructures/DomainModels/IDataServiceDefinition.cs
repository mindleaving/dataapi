using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.Validation;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataServiceDefinition")]
    public interface IDataServiceDefinition<TDataServiceDefinitionField> : IId
        where TDataServiceDefinitionField: IDataServiceDefinitionField
    {
        [Required]
        string OwnerInitials { get; }

        [Required]
        string DataType { get; }

        string Filter { get; }

        [Required]
        [NotEmpty]
        List<TDataServiceDefinitionField> Fields { get; }

        [Required]
        [DataReference("DataServiceTarget")]
        IDataServiceTarget Target { get; }
    }
}