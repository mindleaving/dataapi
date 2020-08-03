// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace DataAPI.Service.Objects
{
    public class DataRedirection
    {
        public DataRedirection(string dataType, string sourceSystemId)
        {
            DataType = dataType;
            SourceSystemId = sourceSystemId;
        }

        public string Id => DataType;
        public string DataType { get; private set; }
        public string SourceSystemId { get; private set; }
    }
}
