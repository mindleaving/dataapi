namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class CreateViewResourceDescription : IDataResourceDescription
    {
        public CreateViewResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.CreateView;
        public string DataType { get; }
    }
}