using System;
using System.Collections.Generic;
using System.Text;

namespace Sid.MailKit.Abstractions
{
    public class MailMessage
    {
        public string Subject { get; set; }

        public string Content { get; set; }

        public IList<MailAddress> Tos { get; set; }

        public IList<MailAddress> Bcc { get; set; }
    }
}
