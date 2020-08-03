using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.DomainModels
{
    public interface IId
    {
        [Required]
        string Id { get; }
    }
}