namespace DataAPI.Service.AccessManagement
{
    public interface ISecurityTokenBuilder
    {
        string BuildForUser(User user);
    }
}