using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Distribution
{
    public interface IDataServiceDefinitionField
    {
        [Required]
        string Path { get; }

        string As { get; }
    }
}