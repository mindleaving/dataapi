using DataAPI.DataStructures.DataManagement;

namespace SharedViewModels.ViewModels
{
    public class ProtocolParameterResponseViewModel : NotifyPropertyChangedBase
    {
        public ProtocolParameterResponseViewModel(
            DataCollectionProtocolParameter parameter, 
            ParameterValueViewModelFactory valueViewModelFactory,
            string response = null)
        {
            Name = parameter.Name;
            var value = response ?? parameter.DefaultValue;
            IsMandatory = parameter.IsMandatory;
            ValueViewModel = valueViewModelFactory.Create(parameter.Type, parameter.DataType, value);
            ((IParameterValueViewModel)ValueViewModel).PropertyChanged += ProtocolParameterResponseViewModel_PropertyChanged;
        }

        private void ProtocolParameterResponseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(IParameterValueViewModel.Value))
                return;
            OnPropertyChanged(nameof(Value));
        }

        public string Name { get; }

        public string Value
        {
            get => ((IParameterValueViewModel) ValueViewModel).Value;
            //set => ((IParameterValueViewModel) ValueViewModel).Value = value;
        }

        public bool IsMandatory { get; }
        public object ValueViewModel { get; }
    }
}