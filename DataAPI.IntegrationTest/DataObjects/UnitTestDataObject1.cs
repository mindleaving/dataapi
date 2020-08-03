using System;
using System.Collections.Generic;
using DataAPI.DataStructures;

namespace DataAPI.IntegrationTest.DataObjects
{
    public class UnitTestDataObject1 : IId
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<UnitTestDataObject2> Objects { get; } = new List<UnitTestDataObject2>();
    }
}