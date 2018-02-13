using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sid.MailKit.Abstractions
{
    public class MailAttachment
    {
        public MailAttachment(string fileName, string filePath)
        {
            FilePath = filePath;
            FileName = fileName;
        }

        public MailAttachment(string fileName, byte[] bytes)
        {
            Bytes = bytes;
            FileName = fileName;
        }

        public MailAttachment(string fileName, Stream stream)
        {
            Stream = stream;
            FileName = fileName;
        }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public byte[] Bytes { get; set; }

        public Stream Stream { get; set; }

        public string MediaType { get; set; }

        public string MediaSubtype { get; set; }

        public string ContentId { get; set; }
    }
}
