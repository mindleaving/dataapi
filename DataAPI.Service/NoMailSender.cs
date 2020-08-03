using System.Threading.Tasks;

namespace DataAPI.Service
{
    public class NoMailSender : IMailSender
    {
        public Task SendAsync(string recipient, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}