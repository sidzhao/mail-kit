using Sid.MailKit.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Sid.MailKit
{
    public class MailSender : IMailSender
    {
        public MailSender(MailServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.Host))
            {
                throw new ArgumentNullException(nameof(options.Host));
            }

            if (string.IsNullOrEmpty(options.UserName))
            {
                throw new ArgumentNullException(nameof(options.UserName));
            }

            if (string.IsNullOrEmpty(options.Password))
            {
                throw new ArgumentNullException(nameof(options.Password));
            }

            MailServerOptions = options;
        }

        public MailServerOptions MailServerOptions { get; }

        public virtual void SendEmail(string subject, string content, params string[] tos)
        {
            SendEmail(subject, content, tos.Select(p => new MailAddress {Address = p}).ToArray(), null);
        }

        public virtual void SendEmail(string subject, string content, params MailAddress[] tos)
        {
            SendEmail(subject, content, tos, null);
        }

        public virtual void SendEmail(string subject, string content, MailAddress[] tos, MailAddress[] bccs)
        {
            var mailMessage = BuildMimeMessage(subject, content, tos, bccs);

            using (var client = new SmtpClient())
            {
                client.Connect(MailServerOptions.Host, MailServerOptions.Port, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(MailServerOptions.UserName, MailServerOptions.Password);
                client.Send(mailMessage);
                client.Disconnect(true);
            }
        }

        public virtual Task SendEmailAsync(string subject, string content, params string[] tos)
        {
            return SendEmailAsync(subject, content, tos.Select(p => new MailAddress { Address = p }).ToArray(), null);
        }

        public virtual Task SendEmailAsync(string subject, string content, params MailAddress[] tos)
        {
            return SendEmailAsync(subject, content, tos, null);
        }
        
        public virtual async Task SendEmailAsync(string subject, string content, MailAddress[] tos, MailAddress[] bccs)
        {
            var mailMessage = BuildMimeMessage(subject, content, tos, bccs);

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(MailServerOptions.Host, MailServerOptions.Port, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(MailServerOptions.UserName, MailServerOptions.Password);
                await client.SendAsync(mailMessage);
                await client.DisconnectAsync(true);
            }
        }

        protected virtual MimeMessage BuildMimeMessage(string subject, string content, MailAddress[] tos, MailAddress[] bccs)
        {
            if (tos == null)
            {
                throw new ArgumentNullException(nameof(tos));
            }

            var mailMessage = new MimeMessage();

            mailMessage.From.Add(MailServerOptions.From == null
                ? new MailboxAddress(MailServerOptions.UserName, MailServerOptions.UserName)
                : new MailboxAddress(string.IsNullOrEmpty(MailServerOptions.From.DisplayName) ? MailServerOptions.From.Address : MailServerOptions.From.DisplayName, MailServerOptions.From.Address));
            foreach (var to in tos)
            {
                mailMessage.To.Add(new MailboxAddress(string.IsNullOrEmpty(to.Address) ? to.Address : to.DisplayName, to.Address));
            }
            if (bccs != null)
            {
                foreach (var bcc in bccs)
                {
                    mailMessage.To.Add(new MailboxAddress(string.IsNullOrEmpty(bcc.Address) ? bcc.Address : bcc.DisplayName, bcc.Address));
                }
            }
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("plain") { Text = content };

            return mailMessage;
        }
    }
}
