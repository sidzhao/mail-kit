using System.Threading.Tasks;

namespace Sid.MailKit.Abstractions
{
    public interface IMailSender
    {
        Task SendEmailAsync(string subject, string content, params string[] tos);

        Task SendEmailAsync(MailMessage mailMessage);

        Task<MailSendMessageResponse> SendEmailAndReturnAsync(string subject, string content, params string[] tos);

        Task<MailSendMessageResponse> SendEmailAndReturnAsync(MailMessage mailMessage);
    }
}
