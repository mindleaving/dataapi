using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DomainModels
{
    [DataApiCollection("DataTag")]
    public interface IDataTag<out TDataReference> : IId
        where TDataReference: IDataReference
    {
        [Required]
        TDataReference DataReference { get; }

        [Required]
        string TagName { get; }
    }
}