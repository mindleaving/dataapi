using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DataManagement
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