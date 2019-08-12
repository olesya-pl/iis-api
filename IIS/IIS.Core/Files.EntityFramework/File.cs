using System;

namespace IIS.Core.Files.EntityFramework
{
    public class File
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Contents { get; set; }
        public DateTime UploadTime { get; set; }
        public bool IsTemporary { get; set; }
    }
}
