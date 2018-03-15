using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mandrill;
using Mandrill.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sid.MailKit.Abstractions;
using MailAddress = Sid.MailKit.Abstractions.MailAddress;
using MailMessage = Sid.MailKit.Abstractions.MailMessage;

namespace Sid.MailKit.Mandrill
{
    public class MandrillSender : IMailSender
    {
        public MandrillSender(MandrillOptions options, ILogger<MandrillSender> logger = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            Logger = logger;

            Logger?.LogInformation("Generated mail sender");
        }

        public MandrillOptions Options { get; }

        public ILogger<MandrillSender> Logger { get; }

        public virtual Task SendEmailAsync(string subject, string content, params string[] tos)
        {
            var mailMessage = new MailMessage(subject, content, tos.Select(p => new MailAddress { Address = p }).ToList());

            return SendEmailAsync(mailMessage);
        }

        public virtual async Task SendEmailAsync(MailMessage mailMessage)
        {
            await SendEmailAndReturnAsync(mailMessage);
        }

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
                var api = new MandrillApi(Options.ApiKey);

                var message = TransferToMandrillMessage(mailMessage);

                var mandrillResponse = (await api.Messages.SendAsync(message)).FirstOrDefault();

                if (mandrillResponse == null)
                {
                    response.Status = MailSendMessageResponseStatus.Failed;
                    response.FailedReason = "No response";
                }
                else
                {
                    response.Id = mandrillResponse.Id;
                    response.Status =
                        string.IsNullOrEmpty(mandrillResponse.RejectReason) ? MailSendMessageResponseStatus.Sent
                            : MailSendMessageResponseStatus.Failed;
                    response.FailedReason = mandrillResponse.RejectReason;
                }
            }
            catch (Exception e)
            {
                response.Status = MailSendMessageResponseStatus.Failed;
                response.FailedReason = e.Message;
            }

            return response;
        }

        protected virtual MandrillMessage TransferToMandrillMessage(MailMessage mailMessage)
        {
            var message = new MandrillMessage
            {
                FromEmail = Options.From.Address,
                FromName = Options.From.DisplayName,
                Subject = mailMessage.Subject
            };
            
            // Setting To address
            message.To = new List<MandrillMailAddress>();

            message.To.AddRange(mailMessage.Tos.Select(p => new MandrillMailAddress(p.Address, p.DisplayName)).ToList());

            if (mailMessage.Cc != null && mailMessage.Cc.Any())
            {
                var cc = mailMessage.Cc.Select(p => new MandrillMailAddress(p.Address, p.DisplayName)).ToList();
                cc.ForEach(p=>p.Type = MandrillMailAddressType.Cc);
                message.To.AddRange(cc);
            }

            if (mailMessage.Bcc != null && mailMessage.Bcc.Any())
            {
                var bcc = mailMessage.Bcc.Select(p => new MandrillMailAddress(p.Address, p.DisplayName)).ToList();
                bcc.ForEach(p => p.Type = MandrillMailAddressType.Bcc);
                message.To.AddRange(bcc);
            }

            // Setting email body
            if (mailMessage.IsHtml)
            {
                message.Html = mailMessage.Content;
            }
            else
            {
                message.Text = mailMessage.Content;
            }

            // Setting attachments
            if (mailMessage.Attachments != null && mailMessage.Attachments.Any())
            {
                message.Attachments = mailMessage.Attachments.Select(TransferToMandrillAttachment).ToList();
            }

            // Setting embedded images
            if (mailMessage.LinkedResources != null && mailMessage.LinkedResources.Any())
            {
                message.Images = mailMessage.LinkedResources.Select(TransferToMandrillImage).ToList();
            }

            return message;
        }

        protected virtual MandrillAttachment TransferToMandrillAttachment(MailAttachment attachment)
        {
            var mandrillAttachment = new MandrillAttachment { Name = attachment.FileName };

            if (!string.IsNullOrEmpty(attachment.FilePath))
            {
                mandrillAttachment.Content = File.ReadAllBytes(attachment.FilePath);
            }
            else if (attachment.Bytes != null)
            {
                mandrillAttachment.Content = attachment.Bytes;
            }
            else if (attachment.Stream != null)
            {
                mandrillAttachment.Content = StreamToBytes(attachment.Stream);
            }
            else
            {
                throw new Exception("Attachment content cannot be null.");
            }

            return mandrillAttachment;
        }

        protected virtual MandrillImage TransferToMandrillImage(MailAttachment attachment)
        {
            var mandrillImage = new MandrillImage { Name = attachment.ContentId };

            if (!string.IsNullOrEmpty(attachment.FilePath))
            {
                mandrillImage.Content = File.ReadAllBytes(attachment.FilePath);
            }
            else if (attachment.Bytes != null)
            {
                mandrillImage.Content = attachment.Bytes;
            }
            else if (attachment.Stream != null)
            {
                mandrillImage.Content = StreamToBytes(attachment.Stream);
            }
            else
            {
                throw new Exception("Image content cannot be null.");
            }

            return mandrillImage;
        }

        private byte[] StreamToBytes(Stream stream)
        {
            var bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            return bytes;
        }
    }
}
