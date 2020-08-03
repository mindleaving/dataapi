using System.IO;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;

namespace DataAPI.Client.UtilityFunctions
{
    internal static class DataTransferFunctions
    {
        public static async Task<DataReference> TransferFile(IDataApiClient dataApiClient, string filePath, string id = null)
        {
            if (id == null)
                id = IdGenerator.Sha1HashFromFile(filePath);
            var dataType = nameof(DataBlob);
            if(await dataApiClient.ExistsAsync<DataBlob>(id))
                return new DataReference(dataType, id);
            var dataBlob = new DataBlob(id, new byte[0], Path.GetFileName(filePath));
            id = await dataApiClient.CreateSubmission(dataBlob, x => x.Data, id);
            using(var fileStream = File.OpenRead(filePath))
                await dataApiClient.TransferSubmissionData(dataType, id, fileStream);
            return new DataReference(dataType, id);
        }

        public static async Task<DataReference> TransferBinaryData(IDataApiClient dataApiClient, byte[] data, string id = null)
        {
            if (id == null)
                id = IdGenerator.Sha1HashFromByteArray(data);
            var dataType = nameof(DataBlob);
            if(await dataApiClient.ExistsAsync<DataBlob>(id))
                return new DataReference(dataType, id);
            var dataBlob = new DataBlob(id, new byte[0]);
            id = await dataApiClient.CreateSubmission(dataBlob, x => x.Data, id);
            await dataApiClient.TransferSubmissionData(dataType, id, data);
            return new DataReference(dataType, id);
        }

        public static async Task<DataReference> TransferBinaryData(IDataApiClient dataApiClient, Stream data, string id = null)
        {
            if (id == null)
                id = IdGenerator.FromGuid(); // We cannot be sure that the stream is reversible and hence cannot use it for calculating an ID
            var dataType = nameof(DataBlob);
            var dataBlob = new DataBlob(id, new byte[0]);
            id = await dataApiClient.CreateSubmission(dataBlob, x => x.Data, id);
            await dataApiClient.TransferSubmissionData(dataType, id, data);
            return new DataReference(dataType, id);
        }
    }
}
