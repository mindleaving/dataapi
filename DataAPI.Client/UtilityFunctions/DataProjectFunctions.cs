using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataManagement;

namespace DataAPI.Client.UtilityFunctions
{
    internal static class DataProjectFunctions
    {
        public static async Task CreateDataCollectionProtocol(
            IDataApiClient dataApiClient,
            string protocolName,
            List<DataCollectionProtocolParameter> parameters,
            List<DataPlaceholder> expectedData)
        {
            var protocol = new DataCollectionProtocol(
                protocolName,
                parameters ?? new List<DataCollectionProtocolParameter>(),
                expectedData ?? new List<DataPlaceholder>());
            await dataApiClient.InsertAsync(protocol, protocolName);
        }

        public static async Task CreateDataProject(
            IDataApiClient dataApiClient,
            string dataProjectId,
            IdSourceSystem projectSourceSystem,
            string protocolName,
            Dictionary<string, string> protocolParameterResponses)
        {
            var protocol = await dataApiClient.GetAsync<DataCollectionProtocol>(protocolName);
            var mandatoryResponses = protocol.Parameters
                .Where(x => x.IsMandatory);
            var missingResponses = mandatoryResponses
                .Where(x => !protocolParameterResponses.ContainsKey(x.Name))
                .ToList();
            if (missingResponses.Any())
            {
                var aggregatedMissingParameters = string.Join(", ", missingResponses.Select(x => x.Name));
                throw new ArgumentException($"You are missing parameter responses for the following mandatory parameters: {aggregatedMissingParameters}");
            }


            var globalizedProjectId = dataProjectId;
            try
            {
                var detectedSourceSystem = IdGenerator.GetSourceSystem(dataProjectId);
                if (detectedSourceSystem != projectSourceSystem)
                {
                    throw new ArgumentException($"The provided data project ID has a prefix that is associated with "
                                                + $"source system '{detectedSourceSystem}', but '{projectSourceSystem}' was specified. "
                                                + $"This either means that the '{projectSourceSystem}' system uses an ID-pattern that "
                                                + $"clashes with the ID-convention (PREFIX.<LocalID>)"
                                                + $"or you specified the wrong ID/source system.");
                }
            }
            catch (KeyNotFoundException)
            {
                if(projectSourceSystem != IdSourceSystem.SelfAssigned)
                    globalizedProjectId = IdGenerator.GlobalizeLocalId(projectSourceSystem, dataProjectId);
            }
            var dataProject = new DataProject(
                globalizedProjectId, 
                projectSourceSystem, 
                protocol, 
                protocolParameterResponses);
            await dataApiClient.InsertAsync(dataProject, dataProjectId);
        }

        public static async Task AddToDataProject(
            IDataApiClient dataApiClient,
            string dataProjectId,
            string dataType,
            string id,
            List<DataReference> derivedData = null,
            string filename = null)
        {
            if (!await dataApiClient.ExistsAsync<DataProject>(dataProjectId))
            {
                throw new KeyNotFoundException($"No data project with ID '{dataProjectId}' exists");
            }
            var dataProjectUploadInfo = new DataProjectUploadInfo(
                dataApiClient.LoggedInUsername,
                DateTime.UtcNow,
                dataProjectId,
                new DataReference(dataType, id),
                derivedData,
                filename);
            await dataApiClient.InsertAsync(dataProjectUploadInfo, dataProjectUploadInfo.Id);
        }

        public static async Task AddToDataSet(DataApiClient dataApiClient, string dataSetId, string dataType, string id)
        {
            if (!await dataApiClient.ExistsAsync<DataSet>(dataSetId))
            {
                var dataSet = new DataSet(dataSetId);
                await dataApiClient.InsertAsync(dataSet, dataSetId);
            }
            var dataTag = new DataTag(
                new DataReference(dataType, id), 
                dataSetId);
            await dataApiClient.InsertAsync(dataTag);
        }
    }
}
