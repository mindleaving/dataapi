namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class SubscriptionResourceDescription : IDataResourceDescription
    {
        public SubscriptionResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.SubscribeToData;
        public string DataType { get; }
    }
}
