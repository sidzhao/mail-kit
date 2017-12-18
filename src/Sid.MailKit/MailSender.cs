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

        public virtual void SendEmail(string subject, string content, params string[] tos)
        {
            var mailMessage = new MailMessage(subject, content, tos.Select(p => new MailAddress { Address = p }).ToList());

            SendEmail(mailMessage);
        }

        public virtual void SendEmail(MailMessage mailMessage)
        {
            var mimeMessage = TransferToMimeMessage(mailMessage);
            using (var client = new SmtpClient())
            {
                client.Connect(MailServerOptions.Host, MailServerOptions.Port, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(MailServerOptions.UserName, MailServerOptions.Password);
                client.Send(mimeMessage);
                client.Disconnect(true);
            }
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
                mimeMessage.To.Add(new MailboxAddress(string.IsNullOrEmpty(to.Address) ? to.Address : to.DisplayName, to.Address));
            }
            if (mailMessage.Bcc != null)
            {
                foreach (var bcc in mailMessage.Bcc)
                {
                    mimeMessage.Bcc.Add(new MailboxAddress(string.IsNullOrEmpty(bcc.Address) ? bcc.Address : bcc.DisplayName, bcc.Address));
                }
            }
            mimeMessage.Subject = mailMessage.Subject;
            var body = new TextPart(mailMessage.IsHtml ? "html" : "plain") { Text = mailMessage.Content };

            if (mailMessage.Attachments != null && mailMessage.Attachments.Count > 0)
            {
                var multipart = new Multipart("mixed");
                multipart.Add(body);

                foreach (var a in mailMessage.Attachments)
                {
                    multipart.Add(TransferToMimePart(a));
                }

                mimeMessage.Body = multipart;
            }
            else
            {
                mimeMessage.Body = body;
            }

            return mimeMessage;
        }

        protected virtual MimePart TransferToMimePart(MailAttachment attachment)
        {
            var mediaType = attachment.MediaType;
            var mediaSubtype = attachment.MediaSubtype;

            if (string.IsNullOrEmpty(mediaType) || string.IsNullOrEmpty(mediaSubtype))
            {
                var extesion = Path.GetExtension(attachment.FilePath).ToLower().Substring(1);

                mediaType = this.GetMediaType(extesion);
                mediaSubtype = extesion;
            }

            var mimePart = new MimePart(mediaType, mediaSubtype)
            {
                ContentObject = new ContentObject(File.OpenRead(attachment.FilePath), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(attachment.FilePath)
            };

            return mimePart;
        }

        protected virtual string GetMediaType(string extesion)
        {
            if (extesion.Contains("png") || extesion.Contains("jpg") || extesion.Contains("jpeg") ||
                extesion.Contains("gif") || extesion.Contains("bmp"))
            {
                return "image";
            }
            else
            {
                return "document";
            }
        }
    }
}
