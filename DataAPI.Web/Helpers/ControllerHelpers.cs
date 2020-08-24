using DataAPI.Service.AccessManagement;
using Microsoft.AspNetCore.Http;

namespace DataAPI.Web.Helpers
{
    internal static class ControllerHelpers
    {
        public static string GetUsername(IHttpContextAccessor httpContextAccessor)
        {
            var domainPrefixedUsername = httpContextAccessor.HttpContext.User.Identity.Name;
            if (domainPrefixedUsername == null)
                return "anonymous";
            return UsernameNormalizer.Normalize(domainPrefixedUsername);
        }
    }
}
