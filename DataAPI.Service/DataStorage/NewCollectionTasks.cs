using System.Threading.Tasks;
using DataAPI.Service.AccessManagement;

namespace DataAPI.Service.DataStorage
{
    public class NewCollectionTasks
    {
        private readonly IMailSender mailSender;

        public NewCollectionTasks(IMailSender mailSender)
        {
            this.mailSender = mailSender;
        }

        public void PerformTasks(string collectionName, User createdBy)
        {
            Task.Run(async () =>
            {
                await SendEmail(collectionName, createdBy);
            });
        }

        private async Task SendEmail(string collectionName, User createdBy)
        {
            if(string.IsNullOrEmpty(createdBy.Email))
                return;

            var subject = $"New collection '{collectionName}' created";
            var newCollectionEmailBody =
                $"Dear {createdBy.FirstName},\n"
                + "\n"
                + $"You have just created a new collection '{collectionName}'. Please describe your data type in the data dictionary so others can understand and reuse your data type.\n"
                + "The data dictionary is part of the Wiki and is located here: http://wiki/Data_Dictionary \n"
                + "If you do not have a login already, you first have to register a new user. You will be able to edit the Wiki immediately after.\n"
                + "\n"
                + "If you have any questions please contact <some person or department>. Thank you for contributing to and enriching our data platform.\n"
                + "Best regards,\n"
                + "\t<some person or department>";

            await mailSender.SendAsync(createdBy.Email, subject, newCollectionEmailBody);
        }
    }
}
