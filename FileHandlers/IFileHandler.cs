using System.Threading.Tasks;
using FileHandlers.AdditionalInformation;
using FileHandlers.Objects;

namespace FileHandlers
{
    public interface IFileHandler
    {
        string[] SupportedExtensions { get; }
        bool RequiresAdditionalInformation { get; }
        IAdditionalInformationViewModel BuildAdditionalInformationViewModel();
        Task<FileHandlerResult> Handle(byte[] fileData, string fileName, string dataProjectId, object additionalInformation);
    }
}
