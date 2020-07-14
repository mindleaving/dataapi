namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class UnsubscribeAllResourceDescription : IResourceDescription
    {
        public ResourceType Type { get; } = ResourceType.Unsubscribe;
    }
}
