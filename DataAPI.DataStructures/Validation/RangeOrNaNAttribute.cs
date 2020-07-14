using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class RangeOrNaNAttribute : RangeAttribute
    {
        public RangeOrNaNAttribute(double minimum, double maximum)
            : base(minimum, maximum)
        {
        }
    }
}
