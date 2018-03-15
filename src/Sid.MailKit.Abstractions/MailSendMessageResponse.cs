using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sid.MailKit.Abstractions
{
    public class MailSendMessageResponse
    {
        public string Id { get; set; }

        public MailSendMessageResponseStatus Status { get; set; }

        public string FailedReason { get; set; }
    }
}
