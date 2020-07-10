using System.Collections.Generic;
using System.Linq;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DomainModels;
using Newtonsoft.Json;

namespace DataProcessing.Test.Models
{
    internal class UnitTestInputObject : IId
    {
        [JsonConstructor]
        public UnitTestInputObject(
            string id,
            List<double> values)
        {
            Id = id;
            Values = values;
        }

        public UnitTestInputObject(List<double> values)
            : this(IdGenerator.FromGuid(), values)
        {
        }

        public string Id { get; }
        public List<double> Values { get; }

        public static UnitTestOutputObject CalculateResult(UnitTestInputObject input)
        {
            return new UnitTestOutputObject(input.Values.Sum());
        }
    }
}