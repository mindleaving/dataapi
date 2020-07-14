using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace DataAPI.Client.Test.Models
{
    internal class TestObject1 : IId
    {
        public TestObject1(string id)
        {
            Id = id;
        }

        public string Id { get; }

        [JsonProperty("source_system")]
        public string SourceSystem { get; set; }

        [JsonProperty("source_id")]
        public string SourceId { get; set; }

        public string Name { get; set; }

        public string CreatedBy { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDiscontinued { get; set; }
    }
}
