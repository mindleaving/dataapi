namespace DataAPI.Service.AccessManagement
{
    public class AuthorizationResult
    {
        private AuthorizationResult(bool isAuthorized, User user = null)
        {
            IsAuthorized = isAuthorized;
            User = user;
        }

        public bool IsAuthorized { get; }
        public User User { get; }

        public static AuthorizationResult Granted(User user)
        {
            return new AuthorizationResult(true, user);
        }

        public static AuthorizationResult Denied()
        {
            return new AuthorizationResult(false);
        }
    }
}
