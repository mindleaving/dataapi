using System.Threading.Tasks;

namespace DataServicesApp.Workflow
{
    public interface ISqlExpressionValidator
    {
        Task<(bool,string)> ValidateWhereAsync(string dataType, string filter);
    }
}
