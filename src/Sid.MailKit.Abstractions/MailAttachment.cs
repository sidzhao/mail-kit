using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sid.MailKit.Abstractions
{
    public class MailAttachment
    {
        public MailAttachment(string filePath, string mediaType, string mediaSubtype)
        {
            FilePath = filePath;
            MediaType = mediaType;
            MediaSubtype = mediaSubtype;
        }

        public string FilePath { get; set; }

        public string MediaType { get; set; } 

        public string MediaSubtype { get; set; }
    }
}
