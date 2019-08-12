using System;
using System.IO;

namespace IIS.Core.Files
{
    public class FileInfo
    {
        public Guid Id { get; }
        public string Name { get; }
        public string ContentType { get; }
        public Stream Contents { get; }
        public byte[] ContentBytes
        {
            get
            {
                if (Contents is MemoryStream ms)
                    return ms.ToArray();
                throw new NotImplementedException();
            }
        }

        public FileInfo(Guid id, string name, string contentType, Stream contents)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Contents = contents;
        }
    }
}
