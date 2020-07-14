namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class DeleteNotificationResourceDescription : IResourceDescription
    {
        public DeleteNotificationResourceDescription(string notificationUsername)
        {
            NotificationUsername = notificationUsername;
        }

        public ResourceType Type => ResourceType.DeleteNotification;
        public string NotificationUsername { get; }
    }
}
