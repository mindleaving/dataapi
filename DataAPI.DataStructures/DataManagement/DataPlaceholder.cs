using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataPlaceholder : IDataPlaceholder
    {
        public DataPlaceholder(
            string dataType, 
            string name, 
            string description, 
            bool isMandatory)
        {
            DataType = dataType;
            Name = name;
            Description = description;
            IsMandatory = isMandatory;
        }

        [Required]
        public string DataType { get; private set; }
        [Required]
        public string Name { get; private set; }
        public string Description { get; private set; }
        [Required]
        public bool IsMandatory { get; private set; }
    }
}
