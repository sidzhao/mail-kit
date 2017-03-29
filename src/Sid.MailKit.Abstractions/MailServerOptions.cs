using Newtonsoft.Json;

namespace Sid.MailKit.Abstractions
{
    public class MailServerOptions
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }
        
        [JsonIgnore]
        public string Password { get; set; }

        public MailAddress From { get; set; }
    }
}
