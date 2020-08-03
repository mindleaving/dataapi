using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCompleteMatchers;
using Commons.Extensions;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.DataManagement;
using SharedViewModels.Repositories;

namespace SharedViewModels.ViewModels
{
    public class ProtocolParameterViewModel : NotifyPropertyChangedBase
    {
        private readonly ParameterValueViewModelFactory valueViewModelFactory;

        public ProtocolParameterViewModel(
            IDataApiClient dataApiClient,
            
            IReadonlyObjectDatabase<DataCollectionProtocolParameter> protocolParameterDatabase,
            DataCollectionProtocolParameter parameter = null)
        {
            valueViewModelFactory = new ParameterValueViewModelFactory(dataApiClient);

            NameAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<DataCollectionProtocolParameter>(
                x => x.Name,
                protocolParameterDatabase,
                new ProtocolParameterAutoCompleteMatcher(),
                allowNewValue: true,
                objectBuilder: name => new DataCollectionProtocolParameter(name, null, false, DataCollectionProtocolParameterType.Text));
            NameAutoCompleteViewModel.PropertyChanged += NameAutoCompleteViewModel_PropertyChanged;
            var dataTypes = Task.Run(async () => await dataApiClient.ListCollectionNamesAsync(true)).Result;
            DataTypeAutoCompleteViewModel = new AutoCompleteTextBoxViewModel<string>(
                x => x,
                new FixedSetDatabase<string>(dataTypes.ToDictionary(x => x, x => x)),
                new StringAutoCompleteMatcher());
            DataTypeAutoCompleteViewModel.PropertyChanged += DataTypeAutoCompleteViewModel_PropertyChanged;
            ValueViewModel = valueViewModelFactory.Create(SelectedParameterType, DataTypeAutoCompleteViewModel.SelectedObject);
            if (parameter != null)
            {
                NameAutoCompleteViewModel.SelectedObject = Task.Run(() => protocolParameterDatabase.GetFromIdAsync(parameter.Id).Result).Result;
                IsMandatory = parameter.IsMandatory;
                SelectedParameterType = parameter.Type;
                DataTypeAutoCompleteViewModel.SelectedObject = parameter.DataType;
                DefaultValue = parameter.DefaultValue;
            }
        }

        private void NameAutoCompleteViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AutoCompleteTextBoxViewModel<DataCollectionProtocolParameter>.SelectedObject))
                return;
            if (NameAutoCompleteViewModel.SelectedObject != null && !NameAutoCompleteViewModel.IsNewValue)
            {
                var existingParameter = NameAutoCompleteViewModel.SelectedObject;
                SelectedParameterType = existingParameter.Type;
                DataTypeAutoCompleteViewModel.SelectedObject = existingParameter.DataType;
            }
        }

        private void DataTypeAutoCompleteViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(AutoCompleteTextBoxViewModel<string>.SelectedObject))
                return;
            ValueViewModel = valueViewModelFactory.Create(SelectedParameterType, DataTypeAutoCompleteViewModel.SelectedObject);
        }

        public AutoCompleteTextBoxViewModel<DataCollectionProtocolParameter> NameAutoCompleteViewModel { get; }

        public IList<DataCollectionProtocolParameterType> ParameterTypes { get; } =
            EnumExtensions.GetValues<DataCollectionProtocolParameterType>().ToList();

        private DataCollectionProtocolParameterType selectedParameterType;
        public DataCollectionProtocolParameterType SelectedParameterType
        {
            get => selectedParameterType;
            set
            {
                selectedParameterType = value;
                ValueViewModel = valueViewModelFactory.Create(SelectedParameterType);
                IsDataTypeSelected = selectedParameterType == DataCollectionProtocolParameterType.DataType;
                OnPropertyChanged();
            }
        }
        private bool isDataTypeSelected;
        public bool IsDataTypeSelected
        {
            get => isDataTypeSelected;
            private set
            {
                isDataTypeSelected = value;
                OnPropertyChanged();
            }
        }

        public AutoCompleteTextBoxViewModel<string> DataTypeAutoCompleteViewModel { get; }

        private object valueViewModel;
        public object ValueViewModel
        {
            get => valueViewModel;
            private set
            {
                valueViewModel = value;
                OnPropertyChanged();
            }
        }

        public string DefaultValue
        {
            get => ((IParameterValueViewModel) ValueViewModel)?.Value;
            set => ((IParameterValueViewModel) ValueViewModel).Value = value;
        }

        private bool isMandatory;
        public bool IsMandatory
        {
            get => isMandatory;
            set
            {
                isMandatory = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(NameAutoCompleteViewModel.SearchText);
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(NameAutoCompleteViewModel.SearchText))
                return false;
            if (SelectedParameterType == DataCollectionProtocolParameterType.DataType)
            {
                if (DataTypeAutoCompleteViewModel.SelectedObject == null)
                    return false;
            }
            return true;
        }

        public DataCollectionProtocolParameter Build()
        {
            return new DataCollectionProtocolParameter(
                NameAutoCompleteViewModel.SearchText, 
                DefaultValue, 
                IsMandatory, 
                SelectedParameterType, 
                DataTypeAutoCompleteViewModel.SelectedObject);
        }
    }
}