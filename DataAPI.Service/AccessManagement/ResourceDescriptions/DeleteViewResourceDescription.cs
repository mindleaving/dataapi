namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class DeleteViewResourceDescription : IDataResourceDescription
    {
        public DeleteViewResourceDescription(string submitter, string dataType)
        {
            Submitter = submitter;
            DataType = dataType;
        }

        public string Submitter { get; }
        public ResourceType Type => ResourceType.DeleteView;
        public string DataType { get; }
    }
}
