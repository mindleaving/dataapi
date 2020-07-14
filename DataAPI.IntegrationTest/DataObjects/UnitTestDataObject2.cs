using System;
using Commons;

namespace DataAPI.IntegrationTest.DataObjects
{
    public class UnitTestDataObject2
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();
        public int Number { get; set; } = StaticRandom.Rng.Next();
    }
}