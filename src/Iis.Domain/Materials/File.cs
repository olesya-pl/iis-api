using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iis.Domain.Materials
{
    public class File
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

        public File(Guid id, string name, string contentType, Stream contents, bool isTemporary)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Contents = contents;
            IsTemporary = isTemporary;
        }

        public File(Guid id) : this(id, null, null, null, false)
        {
        }
    }
}
