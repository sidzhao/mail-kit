﻿using System.Threading.Tasks;

namespace Sid.MailKit.Abstractions
{
    public interface IMailSender
    {
        void SendEmail(string subject, string content, params string[] tos);

        void SendEmail(MailMessage mailMessage);

        Task SendEmailAsync(string subject, string content, params string[] tos);

        Task SendEmailAsync(MailMessage mailMessage);
    }
}
