using Newtonsoft.Json;

namespace DataAPI.DataStructures.UserManagement
{
    public class UserProfile
    {
        [JsonConstructor]
        public UserProfile(
            string username,
            string firstName,
            string lastName,
            string email)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public string Username { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
    }
}
