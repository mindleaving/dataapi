using AutoCompleteMatchers;
using DataAPI.Client;
using DataAPI.Client.Repositories;
using Newtonsoft.Json.Linq;

namespace SharedViewModels.ViewModels
{
    public class DataTypeParameterViewModel : NotifyPropertyChangedBase, IParameterValueViewModel
    {
        public DataTypeParameterViewModel(
            string dataType,
            IDataApiClient dataApiClient,
            
            string value)
        {
            AutoCompleteViewModel = new AutoCompleteTextBoxViewModel<JObject>(
                x => x["Id"].Value<string>(),
                new GenericDatabase(dataApiClient, dataType),
                new GenericIIdAutoCompleteMatcher());
            AutoCompleteViewModel.PropertyChanged += AutoCompleteViewModel_PropertyChanged;
            Value = value;
        }

        private void AutoCompleteViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(AutoCompleteTextBoxViewModel<JObject>.SelectedObject))
                return;
            OnPropertyChanged(nameof(Value));
        }

        public IAutoCompleteTextBoxViewModel AutoCompleteViewModel { get; }

        public string Value
        {
            get
            {
                var jObject = (AutoCompleteViewModel.SelectedDisplayNameObject?.Object as JObject);
                if (jObject == null || !jObject.ContainsKey("Id"))
                    return null;
                return jObject["Id"].Value<string>();
            }
            set => AutoCompleteViewModel.SearchText = value;
        }
    }
}