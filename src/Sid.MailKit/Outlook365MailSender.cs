using Sid.MailKit.Abstractions;

namespace Sid.MailKit
{
    public class Outlook365MailSender : MailSender
    {
        public Outlook365MailSender(string userName, string password)
            : base(
                new MailServerOptions
                {
                    UserName = userName,
                    Password = password,
                    Host = "smtp.office365.com",
                    Port = 587
                })
        {
        }
    }
}
