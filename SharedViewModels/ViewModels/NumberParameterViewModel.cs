using System.Globalization;

namespace SharedViewModels.ViewModels
{
    public class NumberParameterViewModel : NotifyPropertyChangedBase, IParameterValueViewModel
    {
        public NumberParameterViewModel(string value = null)
        {
            Value = value;
        }

        private double number;
        public double Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
            }
        }

        public string Value
        {
            get => Number.ToString(CultureInfo.InvariantCulture);
            set
            {
                try
                {
                    if(value != null)
                        Number = double.Parse(value);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}