using System;
using DataAPI.Client;
using DataAPI.DataStructures.Constants;

namespace SharedViewModels.ViewModels
{
    public class ParameterValueViewModelFactory
    {
        private readonly IDataApiClient dataApiClient;
        

        public ParameterValueViewModelFactory(
            IDataApiClient dataApiClient)
        {
            this.dataApiClient = dataApiClient;
            
        }

        public IParameterValueViewModel Create(
            DataCollectionProtocolParameterType selectedParameterType,
            string dataType = null,
            string value = null)
        {
            switch (selectedParameterType)
            {
                case DataCollectionProtocolParameterType.Text:
                    return new TextParameterViewModel(value);
                case DataCollectionProtocolParameterType.Number:
                    return new NumberParameterViewModel(value);
                case DataCollectionProtocolParameterType.Date:
                    return new DateParameterViewModel(value);
                case DataCollectionProtocolParameterType.UnitValue:
                    return new UnitValueParameterViewModel(value);
                case DataCollectionProtocolParameterType.DataType:
                    var selectedDataType = dataType;
                    return selectedDataType != null 
                        ? new DataTypeParameterViewModel(selectedDataType, dataApiClient, value) 
                        : null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedParameterType), selectedParameterType, null);
            }
        }
    }
}
