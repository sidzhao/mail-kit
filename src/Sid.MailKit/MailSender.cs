using Sid.MailKit.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json;
using System.IO;

namespace Sid.MailKit
{
    public class MailSender : IMailSender
    {
        public MailSender(MailServerOptions options, ILogger<MailSender> logger = null)
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
            Logger = logger;

            Logger?.LogInformation("Generated mail sender");
        }

        public MailServerOptions MailServerOptions { get; }

        public ILogger<MailSender> Logger { get; }
        
        public Task<MailSendMessageResponse> SendEmailAndReturnAsync(string subject, string content, params string[] tos)
        {
            var mailMessage = new MailMessage(subject, content, tos.Select(p => new MailAddress { Address = p }).ToList());

            return SendEmailAndReturnAsync(mailMessage);
        }

        public async Task<MailSendMessageResponse> SendEmailAndReturnAsync(MailMessage mailMessage)
        {
            var response = new MailSendMessageResponse();

            try
            {
                await SendEmailAsync(mailMessage);

                response.Status = MailSendMessageResponseStatus.Sent;
            }
            catch(Exception e)
            {
                response.Status = MailSendMessageResponseStatus.Failed;
                response.FailedReason = e.Message;
            }

            return response;
        }

        public virtual Task SendEmailAsync(string subject, string content, params string[] tos)
        {
            var mailMessage = new MailMessage(subject, content, tos.Select(p => new MailAddress { Address = p }).ToList());

            return SendEmailAsync(mailMessage);
        }

        public virtual async Task SendEmailAsync(MailMessage mailMessage)
        {
            var mimeMessage = TransferToMimeMessage(mailMessage);

            try
            {
                Logger?.LogInformation($"Started to send mail - {JsonConvert.SerializeObject(MailServerOptions)}");

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(MailServerOptions.Host, MailServerOptions.Port, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(MailServerOptions.UserName, MailServerOptions.Password);
                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);
                }

                Logger?.LogInformation("Sent mail successful");
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Sent mail failed - {ex.Message}");
                throw;
            }
        }

        protected virtual MimeMessage TransferToMimeMessage(MailMessage mailMessage)
        {
            if (mailMessage == null)
            {
                throw new ArgumentNullException(nameof(mailMessage));
            }

            if (!mailMessage.Tos.Any())
            {
                throw new Exception("At least one to mail address");
            }

            Logger?.LogInformation($"Build mime message - {JsonConvert.SerializeObject(mailMessage)}");

            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(MailServerOptions.From == null
                ? new MailboxAddress(MailServerOptions.UserName, MailServerOptions.UserName)
                : new MailboxAddress(string.IsNullOrEmpty(MailServerOptions.From.DisplayName) ? MailServerOptions.From.Address : MailServerOptions.From.DisplayName, MailServerOptions.From.Address));

            foreach (var to in mailMessage.Tos)
            {
                mimeMessage.To.Add(new MailboxAddress(string.IsNullOrEmpty(to.DisplayName) ? to.Address : to.DisplayName, to.Address));
            }
            if (mailMessage.Cc != null)
            {
                foreach (var cc in mailMessage.Cc)
                {
                    mimeMessage.Cc.Add(new MailboxAddress(string.IsNullOrEmpty(cc.DisplayName) ? cc.Address : cc.DisplayName, cc.Address));
                }
            }
            if (mailMessage.Bcc != null)
            {
                foreach (var bcc in mailMessage.Bcc)
                {
                    mimeMessage.Bcc.Add(new MailboxAddress(string.IsNullOrEmpty(bcc.DisplayName) ? bcc.Address : bcc.DisplayName, bcc.Address));
                }
            }
            mimeMessage.Subject = mailMessage.Subject;

            var body= new BodyBuilder();
            if (mailMessage.IsHtml)
            {
                body.HtmlBody = mailMessage.Content;
            }
            else
            {
                body.TextBody = mailMessage.Content;
            }

            if (mailMessage.Attachments != null && mailMessage.Attachments.Count > 0)
            {
                foreach (var a in mailMessage.Attachments)
                {
                    body.Attachments.Add(TransferToMimePart(a));
                }
            }

            if (mailMessage.LinkedResources != null && mailMessage.LinkedResources.Count > 0)
            {
                foreach (var lr in mailMessage.LinkedResources)
                {
                    body.LinkedResources.Add(TransferToMimePart(lr));
                }
            }

            mimeMessage.Body = body.ToMessageBody();

            return mimeMessage;
        }

        protected virtual MimePart TransferToMimePart(MailAttachment attachment)
        {
            var mimePart = new MimePart
            {
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = attachment.FileName,
                ContentId = attachment.ContentId
            };

            if (!string.IsNullOrEmpty(attachment.FilePath))
            {
                mimePart.Content = new MimeContent(File.OpenRead(attachment.FilePath));
            }
            else if(attachment.Bytes != null)
            {
                mimePart.Content = new MimeContent(new MemoryStream(attachment.Bytes));
            }
            else if (attachment.Stream != null)
            {
                mimePart.Content = new MimeContent(attachment.Stream);
            }
            else
            {
                throw new Exception("Attachment content cannot be null.");
            }

            return mimePart;
        }
    }
}
