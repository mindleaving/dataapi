using System;

namespace DataAPI.DataStructures.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string dataType, string errorText)
            : base($"Validation failed for data type '{dataType}': {errorText}")
        {
        }

        public ValidationException(string dataType, Exception exception)
            : base($"Validation failed for data type '{dataType}': {exception.Message}", exception)
        {
        }
    }
}