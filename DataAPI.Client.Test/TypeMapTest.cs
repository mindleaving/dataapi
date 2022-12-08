using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAPI.DataStructures.Attributes;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.Validation;
using NUnit.Framework;

namespace DataAPI.Client.Test
{
    [TestFixture]
    public class TypeMapTest
    {
        [Test]
        public void GetCollectionNameReturnsRegistererdName()
        {
            var collectionName = "BinaryData";
            DataApiClient.RegisterType<DataBlob>(collectionName);
            try
            {
                Assert.That(DataApiClient.GetCollectionName<DataBlob>(), Is.EqualTo(collectionName));
            }
            finally
            {
                DataApiClient.UnregisterType<DataBlob>();
            }
        }

        [Test]
        public void GetCollectionNameReturnsDefaultIfNothingElseRegistered()
        {
            Assert.That(DataApiClient.GetCollectionName<DataBlob>(), Is.EqualTo("DataBlob"));
        }

        [Test]
        public void GetCollectionNameReturnsClosestCollectionNameAttribute()
        {
            Assert.That(DataApiClient.GetCollectionName<CompletelyOtherNameImplementation>(), Is.EqualTo("ParameterValidation"));
        }

        [Test]
        public void DataApiCollectionNameAttributeIsUsedIfNoOtherRules()
        {
            Assert.That(DataApiClient.GetCollectionName<CollectionNameTaggedClass>(), Is.EqualTo("AttributeCollectionName"));
        }

        [Test]
        public void GetCollectionNameReturnsClassNameWithoutGeneric()
        {
            Assert.That(DataApiClient.GetCollectionName<Number<double>>(), Is.EqualTo("Number"));
        }

        public class Number<T>
        {
            public T X { get; set; }
        }

        private interface ILocalParameterValidation : IParameterValidation<ParameterOption>
        {
        }

        private class CompletelyOtherNameImplementation : ILocalParameterValidation
        {
            public ParameterValidationType ParameterValidationType { get; set; }
            public ParameterValidationOperator ParameterValidationOperator { get; set; }
            public string Formula1 { get; set; }
            public string Formula2 { get; set; }
            public string InputTitle { get; set; }
            public string InputMessage { get; set; }
            public string ErrorTitle { get; set; }
            public string ErrorMessage { get; set; }
            public List<ParameterOption> Options { get; } = new();
        }

        [DataApiCollection("AttributeCollectionName")]
        private class CollectionNameTaggedClass
        {
            public string Name { get; set; }
        }

        private class ParameterOption : IParameterOption
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private enum ParameterValidationType
        {
            Integer = 1,
            Decimal = 2,
            String = 3,
            Date = 4,
            Time = 5,
            List = 6,
            View = 7,
            Custom = 8,
            Datetime = 9
        }

        private enum ParameterValidationOperator
        {
            Between = 1,
            Equal = 2,
            GreaterThan = 3,
            GreaterThanOrEqual = 4,
            LessThan = 5,
            LessThanOrEqual = 6,
            NotBetween = 7,
            NotEqual = 8
        }

        [DataApiCollection("ParameterValidation")]
        private interface IParameterValidation<TParameterOption> : ICollectionName
        {
            [Required]
            ParameterValidationType ParameterValidationType { get; set; }
            [Required]
            ParameterValidationOperator ParameterValidationOperator { get; set; }
            string Formula1 { get; set; }
            string Formula2 { get; set; }
            string InputTitle { get; set; }
            string InputMessage { get; set; }
            string ErrorTitle { get; set; }
            string ErrorMessage { get; set; }

            [RequiredIfPropertyEqualTo(nameof(ParameterValidationType), TypeMapTest.ParameterValidationType.List)]
            List<TParameterOption> Options { get; }
        }

        [DataApiCollection("MyCollection")]
        private interface ICollectionName
        {
            
        }

        private interface IParameterOption
        {
            [Required]
            string Name { get; set; }

            string Description { get; set; }
        }
    }
}
