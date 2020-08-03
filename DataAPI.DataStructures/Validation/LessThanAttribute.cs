using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class LessThanAttribute : ValidationAttribute
    {
        public LessThanAttribute(double upperLimit)
        {
            ReferenceType = ComparisonReferenceType.Static;
            UpperLimit = upperLimit;
        }

        public LessThanAttribute(string propertyName)
        {
            ReferenceType = ComparisonReferenceType.OtherProperty;
            PropertyName = propertyName;
        }

        public ComparisonReferenceType ReferenceType { get; }
        public double UpperLimit { get; }
        public string PropertyName { get; }
    }
}