using Newtonsoft.Json;
using Sid.MailKit.Abstractions;

namespace Sid.MailKit.Mandrill
{
    public class MandrillOptions
    {
        [JsonIgnore]
        public string ApiKey { get; set; }

        public MailAddress From { get; set; }
    }
}
