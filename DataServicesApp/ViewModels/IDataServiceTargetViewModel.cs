using DataAPI.DataStructures.Constants;
using DataAPI.DataStructures.Distribution;

namespace DataServicesApp.ViewModels
{
    public interface IDataServiceTargetViewModel
    {
        DataServiceTargetType Type { get; }
        string Description { get; }
        bool IsValid(out string errorText);
        IDataServiceTarget Build();
    }
}