using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DataAPI.DataStructures.PostBodies
{
    public class SearchBody
    {
        [JsonConstructor]
        public SearchBody(string query, ResultFormat format)
        {
            Query = query;
            Format = format;
        }

        [Required]
        public string Query { get; }

        [Required]
        public ResultFormat Format { get; }
    }
}
