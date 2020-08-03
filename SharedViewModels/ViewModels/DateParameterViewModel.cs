using System;

namespace SharedViewModels.ViewModels
{
    public class DateParameterViewModel : NotifyPropertyChangedBase, IParameterValueViewModel
    {
        public DateParameterViewModel(string value = null)
        {
            Value = value;
        }

        private DateTime? date;
        public DateTime? Date
        {
            get => date;
            set
            {
                date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
            }
        }
        public string Value
        {
            get => Date?.ToString("yyyy-MM-dd");
            set
            {
                try
                {
                    Date = DateTime.Parse(value);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}