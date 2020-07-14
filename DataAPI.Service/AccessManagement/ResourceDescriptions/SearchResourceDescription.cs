namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class SearchResourceDescription : IDataResourceDescription
    {
        public SearchResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.GetData;
        public string DataType { get; }
    }
}
