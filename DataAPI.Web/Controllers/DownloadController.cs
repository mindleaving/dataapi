using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAPI.DataStructures;
using DataAPI.DataStructures.DataIo;
using DataAPI.DataStructures.DataManagement;
using DataAPI.Service;
using DataAPI.Service.AccessManagement;
using DataAPI.Service.AccessManagement.ResourceDescriptions;
using DataAPI.Service.DataRouting;
using DataAPI.Service.DataStorage;
using DataAPI.Service.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DataAPI.Web.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
#pragma warning disable 1591
    public class DownloadController : Controller
#pragma warning restore 1591
    {
        private readonly IDataRouter dataRouter;
        private readonly AuthorizationModule authorizationModule;
        private readonly IEventLogger apiEventLogger;

#pragma warning disable 1591
        public DownloadController(
#pragma warning restore 1591
            IDataRouter dataRouter, 
            AuthorizationModule authorizationModule,
            IEventLogger apiEventLogger)
        {
            this.dataRouter = dataRouter;
            this.authorizationModule = authorizationModule;
            this.apiEventLogger = apiEventLogger;
        }

        /// <summary>
        /// Get file from either ShortID or from DataType-ID-pair.
        /// ShortID takes presedence over DataType-ID-pair.
        /// </summary>
        [HttpGet]
        [ActionName(nameof(GetFile))]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetFile([FromQuery]string dataType, [FromQuery]string id, [FromQuery] string shortId)
        {
            if (!string.IsNullOrEmpty(shortId))
            {
                var shortIdDataStorage = await dataRouter.GetSourceSystemAsync(nameof(ShortId));
                var shortIdContainer = await shortIdDataStorage.GetFromIdAsync(nameof(ShortId), shortId);
                if (shortIdContainer == null)
                    return NotFound();
                var shortIdJson = DataEncoder.DecodeToJson(shortIdContainer.Data);
                var shortIdObject = JsonConvert.DeserializeObject<ShortId>(shortIdJson);
                dataType = shortIdObject.CollectionName;
                id = shortIdObject.OriginalId;
            }

            // Validate
            if (string.IsNullOrEmpty(dataType))
                return BadRequest("Data type not specified");
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID not specified");

            // Authorize
            var loggedInUsername = UsernameNormalizer.Normalize(HttpContext.User.Identity.Name);
            var resourceDescription = new GetDataResourceDescription(dataType);
            var authorizationResult = await authorizationModule.AuthorizeAsync(resourceDescription, loggedInUsername);
            if (!authorizationResult.IsAuthorized)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

            IRdDataStorage rdDataStorage;
            try
            {
                rdDataStorage = await dataRouter.GetSourceSystemAsync(dataType);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }

            if (!await rdDataStorage.ExistsAsync(dataType, id))
                return NotFound();

            // Provide
            apiEventLogger.Log(LogLevel.Info, $"User '{authorizationResult.User.UserName}' has accessed binary payload of type '{dataType}' with ID '{id}'");
            if (!(rdDataStorage is IBinaryRdDataStorage binaryRdDataStorage))
            {
                var metadata = await rdDataStorage.GetFromIdAsync(dataType, id);
                var metadataJson = DataEncoder.DecodeToJson(metadata.Data);
                var filename = GetFilename(dataType, id, metadataJson);
                return new FileContentResult(Encoding.UTF8.GetBytes(metadataJson), Conventions.JsonContentType)
                {
                    FileDownloadName = filename
                };
            }
            else
            {
                var stream = await binaryRdDataStorage.GetBinaryDataFromIdAsync(dataType, id);
                var metadata = await binaryRdDataStorage.GetMetadataFromId(dataType, id);
                var metadataJson = DataEncoder.DecodeToJson(metadata.Data);
                var filename = GetFilename(dataType, id, metadataJson);
                return new FileStreamResult(stream, Conventions.OctetStreamContentType)
                {
                    FileDownloadName = filename
                };
            }
        }

        private static string GetFilename(string dataType, string id, string metadataJson)
        {
            switch (dataType)
            {
                case nameof(DataBlob):
                    var dataBlob = JsonConvert.DeserializeObject<DataBlob>(metadataJson);
                    return dataBlob.Filename ?? dataBlob.Id + ".bin";
                case nameof(Image):
                    var image = JsonConvert.DeserializeObject<Image>(metadataJson);
                    return image.Filename ?? image.Id + image.Extension;
                default:
                    return $"{dataType}_{id}.json";
            }
        }
    }
}
