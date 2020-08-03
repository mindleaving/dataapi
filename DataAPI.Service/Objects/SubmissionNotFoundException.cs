using System;

namespace DataAPI.Service.Objects
{
    public class SubmissionNotFoundException : Exception
    {
        public SubmissionNotFoundException(string dataType, string id)
            : base($"Could not find submission with ID '{id}' for data type '{dataType}'")
        {
        }
    }
}
