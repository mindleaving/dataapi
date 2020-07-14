using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.DataManagement;

namespace SharedViewModels.ViewModels
{
    public class ImageFileViewModel : IDerivedFileDataViewModel
    {
        public ImageFileViewModel(
            DataReference rawDataReference,
            IObjectDatabase<ShortId> shortIdDatabase,
            IDataApiClient dataApiClient)
        {
            ShortIdEditViewModel = new ShortIdEditViewModel(rawDataReference, shortIdDatabase, dataApiClient);
        }

        public ShortIdEditViewModel ShortIdEditViewModel { get; }
    }
}
