namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class ManageUserResourceDescription : IResourceDescription
    {
        public ManageUserResourceDescription(string userToManage, UserManagementActionType actionType)
        {
            UserToManage = userToManage;
            ActionType = actionType;
        }

        public string UserToManage { get; }
        public UserManagementActionType ActionType { get; }
        public ResourceType Type => ResourceType.ManageUser;
    }

    public enum UserManagementActionType
    {
        ChangePassword = 1,
        AssignRole = 2,
        Delete = 3
    }
}
