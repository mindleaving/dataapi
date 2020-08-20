using System;
using System.Collections.Generic;
using System.IO;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using DataAPI.DataStructures.DataSubscription;
using DataAPI.DataStructures.PostBodies;
using DataAPI.DataStructures.UserManagement;
using DataAPI.DataStructures.Validation;
using DataAPI.DataStructures.Views;
using TypescriptGenerator;

namespace DataAPI.Web
{
    public static class TypescriptGeneration
    {
        private static readonly Dictionary<string, string> RepositoryPaths = new Dictionary<string, string>
        {
            { "stationary-win8", @"G:\Projects\dataapi" }
        };

        public static void Generate()
        {
            var outputDirectory = Path.Combine(
                RepositoryPaths[Environment.MachineName],
                @"DataAPI.Web\frontend\src\types");
            TypescriptGenerator.TypescriptGenerator.Builder
                .Include<CollectionInformation>()
                .Include<UserProfile>()
                .Include<RegistrationInformation>()
                .Include<ChangePasswordBody>()
                .Include<SubmitDataBody>()
                .Include<SearchBody>()
                .Include<SubscriptionBody>()
                .Include<SubmitValidatorBody>()
                .Include<ApplyValidatorBody>()
                .Include<CreateViewBody>()
                .Include<ValidatorDefinition>()
                .Include<SubscriptionInfo>()
                .Include<SubscriptionNotification>()
                .Include<DataProject>()
                .Include<DataProjectUploadInfo>()
                .Include<DataTag>()
                .Include<DataBlob>()
                .Include<DeleteResult>()
                .Include<RulesetValidationResult>()
                .Include<ValidationResult>()
                .Include<ViewInformation>()
                .Include<Image>()
                .ReactDefaults()
                .SetOutputDirectory(outputDirectory)
                .SetDefaultFilenameForInterfaces("dataApiDataStructures.d.ts")
                .Generate();
        }
    }
}
