namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public class DeleteDataResourceDescription : IDataResourceDescription
    {
        public DeleteDataResourceDescription(
            string dataType, 
            string submitterOfObjectToBeDeleted,
            bool overwritingAllowed)
        {
            DataType = dataType;
            SubmitterOfObjectToBeDeleted = submitterOfObjectToBeDeleted;
            OverwritingAllowed = overwritingAllowed;
        }

        public string DataType { get; }
        public string SubmitterOfObjectToBeDeleted { get; }
        public bool OverwritingAllowed { get; }
        public ResourceType Type => ResourceType.DeleteData;
    }
}
