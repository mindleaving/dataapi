using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class GreaterThanAttribute : ValidationAttribute
    {
        public GreaterThanAttribute(double lowerLimit)
        {
            ReferenceType = ComparisonReferenceType.Static;
            LowerLimit = lowerLimit;
        }

        public GreaterThanAttribute(string propertyName)
        {
            ReferenceType = ComparisonReferenceType.OtherProperty;
            PropertyName = propertyName;
        }

        public ComparisonReferenceType ReferenceType { get; }
        public double LowerLimit { get; }
        public string PropertyName { get; }
    }
}
