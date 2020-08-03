using System.Collections.Generic;
using DataAPI.DataStructures;

namespace DataAPI.IntegrationTest.DataObjects
{
    public class UnitTestSearchObject : IId
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public List<Product> Products { get; set; }

        public class Product
        {
            public string Title { get; set; }
            public List<string> Categories { get; set; }
            public double Price { get; set; }
        }
    }
}
