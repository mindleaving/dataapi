using System;

namespace DataProcessing
{
    public class InputTypeRegistrationEventArgs : EventArgs
    {
        public InputTypeRegistrationEventArgs(string dataType)
        {
            DataType = dataType;
        }

        public string DataType { get; }
    }
}