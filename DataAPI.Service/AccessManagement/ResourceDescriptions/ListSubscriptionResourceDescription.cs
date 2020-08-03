namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class ListSubscriptionResourceDescription : IResourceDescription
    {
        public ResourceType Type { get; } = ResourceType.ListSubscriptions;
    }
}
