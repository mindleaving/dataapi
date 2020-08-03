using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    public class DataTag : IDataTag<DataReference>
    {
        [JsonConstructor]
        private DataTag(string id, DataReference dataReference, string tagName)
        {
            Id = id;
            DataReference = dataReference;
            TagName = tagName;
        }

        public DataTag(DataReference dataReference, string tagName)
            : this(IdGenerator.FromGuid(), dataReference, tagName)
        {
        }

        [Required]
        public string Id { get; private set; }
        [Required]
        public DataReference DataReference { get; private set; }
        [Required]
        public string TagName { get; private set; }
    }
}
