using System.Collections.Generic;
using System.Windows.Input;
using FileHandlers.AdditionalInformation;
using SharedViewModels.ViewModels;

namespace FileUploader.ViewModel
{
    public class AdditionalInformationViewModel : NotifyPropertyChangedBase
    {
        public AdditionalInformationViewModel(string filename, List<IAdditionalInformationViewModel> viewModels)
        {
            Filename = filename;
            ViewModels = viewModels;

            OkCommand = new RelayCommand<IClosable>(closable => closable.Close(true), IsAllInformationProvided);
            CancelCommand = new RelayCommand<IClosable>(closable => closable.Close(false));
        }

        public string Filename { get; }

        public List<IAdditionalInformationViewModel> ViewModels { get; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        private bool IsAllInformationProvided(IClosable closable)
        {
            return ViewModels.TrueForAll(x => x.IsAllInformationProvided);
        }
    }
}
