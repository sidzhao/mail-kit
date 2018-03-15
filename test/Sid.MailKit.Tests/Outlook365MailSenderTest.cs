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
        private const string Username = "username";
        private const string Password = "password";
        private const string Address = "emailaddress";

        [Fact]
        public async Task TestSendEmail()
        {
            var mailSender = new Outlook365MailSender(Username, Password);
            
            await mailSender.SendEmailAsync("test", "test async is ok", Address);
        }

        [Fact]
        public async Task TestSendEmailWithAttachments()
        {
            var mailSender = new Outlook365MailSender(Username, Password);
            
            var mailMessage = new MailMessage("test", @"test is ok ", new List<MailAddress>
                {
                    new MailAddress{Address = Address }
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
            
            await mailSender.SendEmailAsync(mailMessage);
        }

        [Fact]
        public async Task TestSendEmailUsingHtml()
        {
            var mailSender = new Outlook365MailSender(Username, Password);

            var imageContentId = "logo";
            var mailMessage = new MailMessage("test", @"<strong>Test<br/> is</br> ok</strong><img src=""cid:logo"">", new List<MailAddress>
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
                new MailAttachment("logo.png", @"D:\\logo.png") {ContentId = imageContentId}
            };
            
            await mailSender.SendEmailAsync("test", "test async is ok", Address);
        }
    }
}
