namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetDataResourceDescription : IDataResourceDescription
    {
        public GetDataResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.GetData;
        public string DataType { get; }
    }
}