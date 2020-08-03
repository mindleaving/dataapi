namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class UnsubscribeResourceDescription : IResourceDescription
    {
        public UnsubscribeResourceDescription(string subscriptionUsername)
        {
            SubscriptionUsername = subscriptionUsername;
        }

        public ResourceType Type { get; } = ResourceType.Unsubscribe;

        public string SubscriptionUsername { get; }
    }
}
