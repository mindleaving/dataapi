using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.DomainModels
{
    public interface IDataServiceDefinitionField
    {
        [Required]
        string Path { get; }

        string As { get; }
    }
}