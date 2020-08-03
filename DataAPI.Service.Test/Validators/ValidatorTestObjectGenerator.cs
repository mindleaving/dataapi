using System;
using System.Linq;
using Commons;

namespace DataAPI.Service.Test.Validators
{
    internal static class ValidatorTestObjectGenerator
    {
        private const double MaxPrice = 100;
        public const int CategoryCount = 5;
        public const int ProductCount = 2;
        public const int CategoriesPerProduct = 2;

        public static ValidatorTestObject GenerateTestObject()
        {
            var categories = Enumerable.Range(0, CategoryCount).Select(_ => Guid.NewGuid().ToString()).ToList();
            return new ValidatorTestObject
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                EmptyString = string.Empty,
                Categories = categories,
                Count = StaticRandom.Rng.Next(),
                Products = Enumerable.Range(0, ProductCount)
                    .Select(idx => new ValidatorTestObject.Product
                    {
                        Price = MaxPrice * StaticRandom.Rng.NextDouble(),
                        Title = Guid.NewGuid().ToString(),
                        Categories = Enumerable.Range(0, CategoriesPerProduct)
                            .Select(categoryIdx => categories[StaticRandom.Rng.Next(categories.Count)])
                            .ToList()
                    }).ToList()
            };
        }
    }
}