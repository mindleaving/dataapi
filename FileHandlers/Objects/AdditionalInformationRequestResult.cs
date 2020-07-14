using System.Collections.Generic;

namespace FileHandlers.Objects
{
    public class AdditionalInformationRequestResult
    {
        public AdditionalInformationRequestResult(bool isRequestCompleted, Dictionary<string, object> additionalInformationObjects)
        {
            IsRequestCompleted = isRequestCompleted;
            AdditionalInformationObjects = additionalInformationObjects;
        }

        /// <summary>
        /// Whether or not requested information was entered.
        /// False, if user cancels request.
        /// </summary>
        public bool IsRequestCompleted { get; }

        public Dictionary<string, object> AdditionalInformationObjects { get; }
    }
}
