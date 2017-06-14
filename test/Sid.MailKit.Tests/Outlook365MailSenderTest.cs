using Sid.MailKit.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sid.MailKit.Tests
{
    public class Outlook365MailSenderTest
    {
        [Fact]
        public async Task TestSendEmailForOutlook365()
        {
            var mailSender = new Outlook365MailSender("username", "password");

            mailSender.SendEmail("test", "test sync is ok", "mailaddress");

            await mailSender.SendEmailAsync("test", "test async is ok", "mailaddress");
        }

        [Fact]
        public async Task TestSendEmailWithAttachmentsForOutlook365()
        {
            var mailSender = new Outlook365MailSender("username", "password");

            var mailMessage = new MailMessage("test", "test is ok", new List<MailAddress>
                {
                    new MailAddress{Address = "mailaddress" }
                });
            mailMessage.Attachments = new List<MailAttachment>
                {
                    new MailAttachment("D:\\header-bg.png", "image", "png"),
                    new MailAttachment("D:\\test.doc", "document", "doc")
                };

            mailSender.SendEmail(mailMessage);

            await mailSender.SendEmailAsync(mailMessage);
        }
    }
}
