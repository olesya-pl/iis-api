using System;
using System.IO;

namespace Iis.Domain.Materials
{
    public class FileInfo
    {
        public Guid Id { get; }
        public string Name { get; }
        public string ContentType { get; }
        public Stream Contents { get; }
        public bool IsTemporary { get; }

        public byte[] ContentBytes
        {
            get
            {
                if (Contents is MemoryStream ms)
                    return ms.ToArray();
                throw new NotImplementedException();
            }
        }

        public FileInfo(Guid id, string name, string contentType, Stream contents, bool isTemporary)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Contents = contents;
            IsTemporary = isTemporary;
        }

        public FileInfo(Guid id) : this(id, null, null, null, false)
        {
        }
    }
}
