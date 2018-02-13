using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sid.MailKit.Abstractions
{
    public class MailMessage
    {
        public MailMessage(string subject, string content, IList<MailAddress> tos)
        {
            Subject = subject;
            Content = content;
            Tos = tos;
        }

        public string Subject { get; set; }

        public string Content { get; set; }

        public IList<MailAddress> Tos { get; set; }

        public IList<MailAddress> Cc { get; set; }

        public IList<MailAddress> Bcc { get; set; }

        public IList<MailAttachment> Attachments { get; set; }

        public IList<MailAttachment> LinkedResources { get; set; }

        public bool IsHtml { get; set; } = false;
    }
}
