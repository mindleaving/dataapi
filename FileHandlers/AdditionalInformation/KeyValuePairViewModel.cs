using SharedViewModels.ViewModels;

namespace FileHandlers.AdditionalInformation
{
    public class KeyValuePairViewModel : NotifyPropertyChangedBase
    {
        public KeyValuePairViewModel(string key)
        {
            Key = key;
        }

        public string Key { get; }

        private string value;
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }
    }
}