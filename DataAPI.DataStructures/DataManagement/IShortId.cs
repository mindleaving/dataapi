using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DataManagement
{
    [DataApiCollection("ShortId")]
    public interface IShortId : IId
    {
        /// <summary>
        /// The collection to which the short ID refers
        /// </summary>
        [Required]
        string CollectionName { get; }

        /// <summary>
        /// The ID of the object in the collection
        /// </summary>
        [Required]
        string OriginalId { get; }
    }
}