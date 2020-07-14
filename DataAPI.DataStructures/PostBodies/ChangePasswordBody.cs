using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class ChangePasswordBody
    {
        [JsonConstructor]
        public ChangePasswordBody(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [Required]
        public string Username { get; }

        [Required]
        public string Password { get; }
    }
}
