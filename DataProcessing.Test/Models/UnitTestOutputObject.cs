using DataAPI.DataStructures;
using Newtonsoft.Json;

namespace DataProcessing.Test.Models
{
    internal class UnitTestOutputObject : IId
    {
        [JsonConstructor]
        private UnitTestOutputObject(
            string id,
            double result)
        {
            Id = id;
            Result = result;
        }

        public UnitTestOutputObject(double result)
        : this(IdGenerator.FromGuid(), result)
        {
        }

        public string Id { get; }
        public double Result { get; }
    }
}