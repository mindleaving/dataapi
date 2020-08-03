using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures
{
    public interface IId
    {
        [Required]
        string Id { get; }
    }
}