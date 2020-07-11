using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Commons.Physics;
using DataAPI.DataStructures;
using DataAPI.DataStructures.CollectionManagement;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using DataAPI.DataStructures.Views;
using NUnit.Framework;
using TypeLite;
using TypeLite.Extensions;
using TypeLite.TsModels;

namespace TypescriptGenerator
{
    [TestFixture]
    [Category("Tool")]
    public class Runner
    {
        [Test]
        public void GenerateDataApiDataStructures()
        {
            var definitions = TypeScript.Definitions()
                .For<CollectionInformation>()
                .For<UserProfile>()
                .For<RegistrationInformation>()
                .For<ChangePasswordBody>()
                .For<SubmitDataBody>()
                .For<SearchBody>()
                .For<SubscriptionBody>()
                .For<SubmitValidatorBody>()
                .For<ApplyValidatorBody>()
                .For<CreateViewBody>()
                .For<ValidatorDefinition>()
                .For<SubscriptionInfo>()
                .For<SubscriptionNotification>()
                .For<DataProject>()
                .For<DataProjectUploadInfo>()
                .For<DataTag>()
                .For<DataBlob>()
                .For<DeleteResult>()
                .For<RulesetValidationResult>()
                .For<ValidationResult>()
                .For<ViewInformation>()
                .For<Image>();
            var result = Generate(definitions);
            File.WriteAllText(
                @"G:\Projects\dataapi\DataAPI.Web\frontend\src\types\dataApiDataStructures.d.ts",
                result);
        }

        private string Generate(TypeScriptFluent definitions)
        {
            var result = definitions
                .For<Guid>().Ignore()
                .For<UnitValue>().Ignore()
                .For<CompoundUnit>().Ignore()
                .WithMemberFormatter(CamelCaseFormatter)
                .WithMemberTypeFormatter(TypeFormatter)
                .Generate();
            return PostProcess(result);
        }

        private string PostProcess(string result)
        {
            return result.Replace("declare namespace", "export namespace");
        }

        private string CamelCaseFormatter(TsProperty identifier)
        {
            var camelCaseName = char.ToLowerInvariant(identifier.Name[0]) + identifier.Name.Substring(1);
            if (identifier.IsOptional)
                camelCaseName += "?";
            return camelCaseName;
        }

        private string TypeFormatter(
            TsProperty property,
            string name)
        {
            var desiredName = name;
            if (property.PropertyType.Type == typeof(UnitValue))
                desiredName = "math.Unit";
            else if (property.PropertyType.Type == typeof(Guid))
                desiredName = "string";
            else if (typeof(IDictionary).IsAssignableFrom(property.PropertyType.Type))
                desiredName = "any";
            var isOptional = property.IsOptional
                             || (property.MemberInfo.MemberType == MemberTypes.Property
                                 && ((PropertyInfo) property.MemberInfo).PropertyType.IsNullable());
            if (isOptional)
                desiredName += " | null";
            return property.PropertyType.IsCollection() ? $"{desiredName}[]" : desiredName;
        }

        [OneTimeSetUp]
        public void FixAssemblyException()
        {
            AssemblyRedirect.RedirectAssembly();
        }
    }
}
