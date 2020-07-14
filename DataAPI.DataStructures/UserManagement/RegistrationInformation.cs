using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.UserManagement
{
    public class RegistrationInformation
    {
        [JsonConstructor]
        public RegistrationInformation(
            string username,
            string firstName,
            string lastName,
            string password, 
            string email)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            Email = email;
        }

        [Required]
        public string Username { get; }
        public string FirstName { get; }
        public string LastName { get; }
        [Required]
        [EmailAddress]
        public string Email { get; }
        [Required]
        public string Password { get; }
    }
}
