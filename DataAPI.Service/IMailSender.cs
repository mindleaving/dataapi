using System.Threading.Tasks;

namespace DataAPI.Service
{
    public interface IMailSender
    {
        Task SendAsync(string recipient, string subject, string message);
    }
}