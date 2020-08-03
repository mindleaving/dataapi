using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.Constants;

namespace DataAPI.DataStructures.Distribution
{
    [DataApiCollection("DataServiceTarget")]
    public interface IDataServiceTarget : IId
    {
        [Required]
        DataServiceTargetType Type { get; }
    }
}