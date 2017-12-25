using Sid.MailKit.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Sid.MailKit.Tests
{
    public class Outlook365MailSenderTest
    {
        [Fact]
        public async Task TestSendEmail()
        {
            var mailSender = new Outlook365MailSender("username", "password");

            mailSender.SendEmail("test", "test sync is ok", "mailaddress");

            await mailSender.SendEmailAsync("test", "test async is ok", "mailaddress");
        }

        [Fact]
        public async Task TestSendEmailWithAttachments()
        {
            var mailSender = new Outlook365MailSender("username", "password");

            var mailMessage = new MailMessage("test", "test is ok", new List<MailAddress>
                {
                    new MailAddress{Address = "mailaddress" }
                });

            var fs = new FileStream(@"D:\\test.docx", FileMode.Open, FileAccess.Read);
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);

            mailMessage.Attachments = new List<MailAttachment>
                {
                    new MailAttachment("test.pdf",@"D:\\test.pdf"),
                    new MailAttachment("test.docx",@"D:\\test.docx"),
                    new MailAttachment("test.docx", bytes)
                };

            mailSender.SendEmail(mailMessage);

            await mailSender.SendEmailAsync(mailMessage);
        }

        [Fact]
        public async Task TestSendEmailUsingHtml()
        {
            var mailSender = new Outlook365MailSender("username", "password");

            var mailMessage = new MailMessage("test", "<strong>Test<br/> is</br> ok</strong>", new List<MailAddress>
                {
                    new MailAddress{Address = "mailaddress" }
                })
            {
                IsHtml = true
            };

            mailSender.SendEmail(mailMessage);

            await mailSender.SendEmailAsync("test", "test async is ok", "mailaddress");
        }
    }
}
