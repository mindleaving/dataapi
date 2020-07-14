using System.Collections.Generic;

namespace DataAPI.Service.Test.Validators
{
    public class ValidatorTestObject
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string EmptyString { get; set; }
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
