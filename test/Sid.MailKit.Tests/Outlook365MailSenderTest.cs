using System;
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
    }
}
