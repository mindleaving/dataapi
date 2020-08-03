using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class DataReferenceAttribute : ValidationAttribute
    {
        public DataReferenceAttribute(string dataType)
        {
            DataType = dataType;
        }

        public string DataType { get; }
    }
}
