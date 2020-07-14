using System.Collections.Generic;

namespace DataAPI.Client.Test.Models
{
    internal class TestObject2
    {
        public string Id { get; set; }
        public string EntityId { get; set; }
        public List<WellMeasurement> Items { get; }
        public string ExperimentId { get; set; }
    }

    internal class WellMeasurement
    {
        public string ItemId { get; set; }
    }
}
