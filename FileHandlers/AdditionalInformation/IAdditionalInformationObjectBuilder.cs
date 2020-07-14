using System.Collections.Generic;

namespace FileHandlers.AdditionalInformation
{
    public interface IAdditionalInformationObjectBuilder
    {
        object Build(Dictionary<string, string> keyValuePairs);
    }
}