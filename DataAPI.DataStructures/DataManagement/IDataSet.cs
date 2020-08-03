using DataAPI.DataStructures.Attributes;

namespace DataAPI.DataStructures.DataManagement
{
    /// <summary>
    /// A better name for this class might be DataTagDescription,
    /// because data is not directly associated with a data set but instead tagged.
    /// All data with a specific tag constitute a DataSet.
    /// </summary>
    [DataApiCollection("DataSet")]
    public interface IDataSet : IId
    {
        string Description { get; set; }
    }
}