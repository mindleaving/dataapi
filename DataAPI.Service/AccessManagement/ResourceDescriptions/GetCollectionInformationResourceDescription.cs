namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetCollectionInformationResourceDescription : IDataResourceDescription
    {
        public GetCollectionInformationResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.ViewCollectionInformation;
        public string DataType { get; }
    }
}
