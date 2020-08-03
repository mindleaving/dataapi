using System;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Misc;
using DataAPI.Service.Objects;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace DataAPI.Service
{
    public class MailSender : IMailSender
    {
        private readonly string mailServerAddress;
        private readonly ushort mailServerPort;
        private readonly MailboxAddress sender;
        private readonly bool useSSL;
        private readonly bool isAuthenticationRequired;
        private readonly string mailPasswordEnvironmentVariableName;
        private readonly IEventLogger eventLogger;

        public MailSender(
            string mailServerAddress, 
            ushort mailServerPort, 
            string sender, 
            bool useSSL,
            bool isAuthenticationRequired,
            string mailPasswordEnvironmentVariableName,
            IEventLogger eventLogger)
        {
            this.mailServerAddress = mailServerAddress;
            this.mailServerPort = mailServerPort;
            this.sender = MailboxAddress.Parse(sender);
            this.useSSL = useSSL;
            this.isAuthenticationRequired = isAuthenticationRequired;
            this.mailPasswordEnvironmentVariableName = mailPasswordEnvironmentVariableName;
            this.eventLogger = eventLogger;
        }

        public Task SendAsync(string recipient, string subject, string message)
        {
            return Task.Run(() =>
                {
                    try
                    {
                        var mail = new MimeMessage();
                        mail.From.Add(sender);
                        mail.To.Add(MailboxAddress.Parse(recipient));
                        mail.Subject = subject;
                        mail.Body = new TextPart(TextFormat.Plain)
                        {
                            Text = message
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect(mailServerAddress, mailServerPort);
                            if(isAuthenticationRequired)
                                smtpClient.Authenticate(sender.Address, Secrets.Get(mailPasswordEnvironmentVariableName));
                            smtpClient.Send(mail);
                            smtpClient.Disconnect(true);
                        }
                        eventLogger.Log(LogLevel.Info, $"Successfully sent email to {recipient} with subject '{subject}'");
                    }
                    catch (Exception e)
                    {
                        eventLogger.Log(LogLevel.Error, $"Could not send email to {recipient}: " + e.InnermostException().Message);
                    }
                });
        }

        //public Task SendAsync(string recipient, string subject, string message)
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            using (var smtpClient = new SmtpClient(mailServerAddress, mailServerPort)
        //            {
        //                DeliveryMethod = SmtpDeliveryMethod.Network,
        //                EnableSsl = useSSL,
        //                UseDefaultCredentials = false,
        //                Credentials = isAuthenticationRequired ? new NetworkCredential(sender.Address, Secrets.Get(mailPasswordEnvironmentVariableName)) : null
        //            })
        //            {
        //                using (var mail = new MailMessage(sender.Address, recipient, subject, message))
        //                {
        //                    smtpClient.Send(mail);
        //                }
        //            }
        //            eventLogger.Log(LogLevel.Info, $"Successfully sent email to {recipient} with subject '{subject}'");
        //        }
        //        catch (Exception e)
        //        {
        //            eventLogger.Log(LogLevel.Error, $"Could not send email to {recipient}: " + e.InnermostException().Message);
        //        }
        //    });
        //}
    }
}
