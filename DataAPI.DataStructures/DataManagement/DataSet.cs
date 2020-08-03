using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.DataManagement
{
    /// <summary>
    /// A better name for this class might be DataTagDescription,
    /// because data is not directly associated with a data set but instead tagged.
    /// All data with a specific tag constitute a DataSet.
    /// </summary>
    public class DataSet : IDataSet
    {
        [JsonConstructor]
        public DataSet(string id)
        {
            Id = id;
        }

        [Required]
        public string Id { get; private set; }
        public string Description { get; set; }
    }
}
