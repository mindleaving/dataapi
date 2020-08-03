using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.DataManagement
{
    public interface IDataPlaceholder
    {
        [Required]
        string DataType { get; }

        [Required]
        string Name { get; }
        
        string Description { get; }

        [Required]
        bool IsMandatory { get; }
    }
}