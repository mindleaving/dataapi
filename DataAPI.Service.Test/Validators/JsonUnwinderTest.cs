using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DataAPI.Service.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DataAPI.Service.Test.Validators
{
    [TestFixture]
    public class JsonUnwinderTest
    {
        [Test]
        [TestCase("Count", 1)]
        [TestCase("Products.Price", ValidatorTestObjectGenerator.ProductCount)]
        [TestCase("Products.Categories.Name", ValidatorTestObjectGenerator.ProductCount * ValidatorTestObjectGenerator.CategoriesPerProduct)]
        [TestCase("Categories.Name", ValidatorTestObjectGenerator.CategoryCount)]
        public void ObjectIsFullyUnwoundAlongDefinedPath(string path, int expectedObjects)
        {
            var testObjectJson = JsonConvert.SerializeObject(ValidatorTestObjectGenerator.GenerateTestObject());
            var testJObject = JObject.Parse(testObjectJson);

            var actual = JsonUnwinder.Unwind(testJObject, path).ToList();
            Assert.That(actual.Count, Is.EqualTo(expectedObjects));
        }

        [Test]
        [Ignore("Debug")]
        [TestCase(@"XXX.json", "Path.To.Property")]
        public void PerformanceTest(string filePath, string propertyPath)
        {
            var bestResult = 36934;
            var json = File.ReadAllText(filePath);
            var jObject = JObject.Parse(json);
            var stopwatch = Stopwatch.StartNew();
            var actual = JsonUnwinder.Unwind(jObject, propertyPath).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(bestResult));
        }
    }
}
