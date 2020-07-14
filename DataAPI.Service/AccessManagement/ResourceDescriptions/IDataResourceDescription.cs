namespace DataAPI.Service.AccessManagement.ResourceDescriptions
{
    public interface IDataResourceDescription : IResourceDescription
    {
        string DataType { get; }
    }
}