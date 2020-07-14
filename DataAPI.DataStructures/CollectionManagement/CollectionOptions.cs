using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.CollectionManagement
{
    public class CollectionOptions
    {
        [JsonConstructor]
        public CollectionOptions(string collectionName)
        {
            CollectionName = collectionName;
        }

        [Required]
        public string CollectionName { get; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool? IsProtected { get; set; }
        public bool? NonAdminUsersCanOverwriteData { get; set; }
        public bool? IsHidden { get; set; }
        public IdGeneratorType? IdGeneratorType { get; set; }
    }
}
