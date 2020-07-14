using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class RequiredIfPropertyInSetAttribute : ValidationAttribute
    {
        public RequiredIfPropertyInSetAttribute(string propertyName, params object[] values)
        {
            PropertyName = propertyName;
            Values = values;
        }

        public string PropertyName { get; }
        public object[] Values { get; }
    }
}
