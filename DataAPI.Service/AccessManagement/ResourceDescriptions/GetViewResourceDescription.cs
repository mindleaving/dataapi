namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetViewResourceDescription : IDataResourceDescription
    {
        public GetViewResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.GetView;
        public string DataType { get; }
    }
}