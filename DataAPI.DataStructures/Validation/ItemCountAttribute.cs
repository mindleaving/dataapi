using System;
using System.ComponentModel.DataAnnotations;

namespace DataAPI.DataStructures.Validation
{
    public class ItemCountAttribute : ValidationAttribute
    {
        public ItemCountAttribute(int exactCount)
        {
            if(exactCount < 0)
                throw new ArgumentOutOfRangeException(nameof(exactCount));
            MinimumCount = exactCount;
            MaximumCount = exactCount;
        }

        public ItemCountAttribute(int min, int max)
        {
            if(min < 0)
                throw new ArgumentOutOfRangeException(nameof(min));
            if(max < 0)
                throw new ArgumentOutOfRangeException(nameof(max));
            if(max < min)
                throw new ArgumentException("Maximum must be greater than or equal to minimum");
            MinimumCount = min;
            MaximumCount = max;
        }

        public int MinimumCount { get; }
        public int MaximumCount { get; }
    }
}