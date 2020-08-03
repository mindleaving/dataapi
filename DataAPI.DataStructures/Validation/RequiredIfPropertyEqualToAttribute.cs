using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class RequiredIfPropertyEqualToAttribute : ValidationAttribute
    {
        public RequiredIfPropertyEqualToAttribute(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; }
        public object Value { get; }
    }
}
