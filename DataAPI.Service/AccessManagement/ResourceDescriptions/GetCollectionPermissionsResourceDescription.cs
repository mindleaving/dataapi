namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetCollectionPermissionsResourceDescription : IResourceDescription
    {
        public ResourceType Type { get; } = ResourceType.GetCollectionPermissions;
    }
}
