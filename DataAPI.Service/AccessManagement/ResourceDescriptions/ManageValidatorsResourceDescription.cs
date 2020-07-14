namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class ManageValidatorsResourceDescription : IDataResourceDescription
    {
        public ManageValidatorsResourceDescription(
            ValidatorManagementAction action, 
            string validatorSubmitter, 
            string dataType)
        {
            Action = action;
            ValidatorSubmitter = validatorSubmitter;
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.ManageValidators;
        public ValidatorManagementAction Action { get; }
        public string ValidatorSubmitter { get; }
        public string DataType { get; }
    }
}