using System;

namespace DataAPI.Service.Objects
{
    public class DocumentAlreadyExistsException : Exception
    {
        public DocumentAlreadyExistsException(string message)
            : base(message)
        {
        }
    }
}
