using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class ShortId : IShortId
    {
        [JsonConstructor]
        public ShortId(string id, string collectionName, string originalId)
        {
            Id = id;
            CollectionName = collectionName;
            OriginalId = originalId;
        }

        public static ShortId FromIId(string shortId, IId iid)
        {
            return new ShortId(shortId, iid.GetType().Name, iid.Id);
        }

        /// <summary>
        /// The short ID
        /// </summary>
        [Required]
        public string Id { get; private set; }

        
        /// <summary>
        /// The collection to which the short ID refers
        /// </summary>
        [Required]
        public string CollectionName { get; private set; }

        /// <summary>
        /// The ID of the object in the collection
        /// </summary>
        [Required]
        public string OriginalId { get; private set; }
    }
}
