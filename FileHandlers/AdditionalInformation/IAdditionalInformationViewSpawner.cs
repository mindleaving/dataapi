using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileHandlers.AdditionalInformation
{
    public interface IAdditionalInformationViewSpawner
    {
        Task<bool?> Spawn(string filename, List<IAdditionalInformationViewModel> viewModels);
    }
}
