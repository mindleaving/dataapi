using System.Threading.Tasks;
using GenericFileUploader.ViewModel;

namespace GenericFileUploader.FileHandlers
{
    public class DataBlobFileHandler : IFileHandler
    {
        public string[] SupportedExtensions { get; }
        public Task<FileViewModelBase> Handle(string filePath)
        {
            throw new System.NotImplementedException();
        }
    }
}