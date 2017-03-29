using System.Threading.Tasks;

namespace Sid.MailKit.Abstractions
{
    public interface IMailSender
    {
        void SendEmail(string subject, string content, params string[] tos);

        void SendEmail(string subject, string content, params MailAddress[] tos);

        void SendEmail(string subject, string content, MailAddress[] tos, MailAddress[] bccs);

        Task SendEmailAsync(string subject, string content, params string[] tos);

        Task SendEmailAsync(string subject, string content, params MailAddress[] tos);

        Task SendEmailAsync(string subject, string content, MailAddress[] tos, MailAddress[] bccs);
    }
}
