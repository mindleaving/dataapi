namespace FileHandlers.AdditionalInformation
{
    public interface IAdditionalInformationViewModel
    {
        string FileHandlerType { get; }
        bool IsAllInformationProvided { get; }
        object BuildAdditionalInformationObject();
    }
}