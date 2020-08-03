namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class AddValidatorResourceDescription : IDataResourceDescription
    {
        public AddValidatorResourceDescription(string dataType)
        {
            DataType = dataType;
        }

        public ResourceType Type => ResourceType.AddValidator;
        public string DataType { get; }
    }
}
