namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class SubmitDataResourceDescription : IDataResourceDescription
    {
        public SubmitDataResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.SubmitData;
        public string DataType { get; }
    }
}