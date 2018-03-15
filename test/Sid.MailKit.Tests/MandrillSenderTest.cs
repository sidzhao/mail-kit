using Sid.MailKit.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sid.MailKit.Mandrill;
using Xunit;

namespace Sid.MailKit.Tests
{
    public class MandrillSenderTest
    {
        private MandrillOptions Options = new MandrillOptions
        {
            ApiKey = "3S_Cvmd0NBPZZ852GfrANA",
            From = new MailAddress { Address = "test@webezi.com.au", DisplayName = "Test Account" }
        };
        private const string Address = "sid.zhao@webezi.com.au";

        [Fact]
        public async Task TestSendEmail()
        {
            var mailSender = new MandrillSender(Options);

            var response = await mailSender.SendEmailAndReturnAsync("test", "test async is ok", Address);

            Assert.Equal(MailSendMessageResponseStatus.Sent, response.Status);
        }

        [Fact]
        public async Task TestSendEmailWithAttachments()
        {
            var mailSender = new MandrillSender(Options);

            var mailMessage = new MailMessage("test with attachment", @"test with attachment is ok ", new List<MailAddress>
            {
                new MailAddress{Address = Address }
            })
            {
                IsHtml = true
            };

            var fs = new FileStream(@"D:\\test.docx", FileMode.Open, FileAccess.Read);
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);

            mailMessage.Attachments = new List<MailAttachment>
            {
                new MailAttachment("test.pdf",@"D:\\test.pdf"),
                new MailAttachment("test.docx",@"D:\\test.docx"),
                new MailAttachment("test.docx", bytes)
            };

            var response = await mailSender.SendEmailAndReturnAsync(mailMessage);

            Assert.Equal(MailSendMessageResponseStatus.Sent, response.Status);
        }

        [Fact]
        public async Task TestSendEmailUsingHtml()
        {
            var mailSender = new MandrillSender(Options);
            
            var mailMessage = new MailMessage("test using html", @"<strong>Test<br/> is</br> ok</strong><img src=""cid:logo"">", new List<MailAddress>
            {
                new MailAddress{Address = Address }
            })
            {
                IsHtml = true
            };

            mailMessage.Attachments = new List<MailAttachment>
            {
                new MailAttachment("test.pdf",@"D:\\test.pdf")
            };

            mailMessage.LinkedResources = new List<MailAttachment>
            {
                new MailAttachment("logo.png", @"D:\\logo.png") {ContentId = "logo"}
            };

            var response = await mailSender.SendEmailAndReturnAsync(mailMessage);

            Assert.Equal(MailSendMessageResponseStatus.Sent, response.Status);
        }
    }
}
