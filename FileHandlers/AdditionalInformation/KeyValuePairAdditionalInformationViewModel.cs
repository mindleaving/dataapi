using System.Collections.Generic;
using System.Linq;

namespace FileHandlers.AdditionalInformation
{
    public class KeyValuePairAdditionalInformationViewModel : IAdditionalInformationViewModel
    {
        private readonly IAdditionalInformationObjectBuilder additionalInformationObjectBuilder;

        public KeyValuePairAdditionalInformationViewModel(
            string fileHandlerType, 
            List<string> keys,
            IAdditionalInformationObjectBuilder additionalInformationObjectBuilder)
        {
            this.additionalInformationObjectBuilder = additionalInformationObjectBuilder;
            FileHandlerType = fileHandlerType;
            KeyValuePairs = keys.Select(x => new KeyValuePairViewModel(x)).ToList();
        }

        public string FileHandlerType { get; }
        public bool IsAllInformationProvided => KeyValuePairs.TrueForAll(kvp => !string.IsNullOrWhiteSpace(kvp.Value));
        public List<KeyValuePairViewModel> KeyValuePairs { get; }

        public object BuildAdditionalInformationObject()
        {
            return additionalInformationObjectBuilder.Build(KeyValuePairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}
