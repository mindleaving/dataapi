namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class GetGlobalRolesResourceDescription : IResourceDescription
    {
        public GetGlobalRolesResourceDescription(string username)
        {
            Username = username;
        }

        public ResourceType Type { get; } = ResourceType.GetGlobalRoles;
        public string Username { get; }
    }
}
