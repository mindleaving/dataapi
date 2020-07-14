using System.Collections.Generic;
using Commons.Physics;

namespace SharedViewModels.ViewModels
{
    public class UnitValueParameterViewModel : NotifyPropertyChangedBase, IParameterValueViewModel
    {
        public UnitValueParameterViewModel(string value = null, IList<IUnitDefinition> supportedUnits = null)
        {
            UnitValueEditViewModel = new UnitValueEditViewModel(supportedUnits: supportedUnits);
            UnitValueEditViewModel.PropertyChanged += UnitValueEditViewModel_PropertyChanged;
            Value = value;
        }

        private void UnitValueEditViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(ViewModels.UnitValueEditViewModel.Amount))
                return;
            OnPropertyChanged(nameof(Value));
        }

        public UnitValueEditViewModel UnitValueEditViewModel { get; }

        public string Value
        {
            get => UnitValueEditViewModel.Amount?.ToString();
            set
            {
                try
                {
                    var amount = UnitValue.Parse(value);
                    UnitValueEditViewModel.Amount = amount;
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}