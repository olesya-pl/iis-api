using System;
using System.IO;

namespace Iis.Services.Contracts.Dtos
{
    public class FileContentResult
    {
        public Guid Id;
        public string Name;
        public string ContentType;
        public Stream Content;
        public static FileContentResult Empty => new FileContentResult { Id = Guid.Empty, Content = Stream.Null };
        public bool IsEmpty => Id.Equals(Guid.Empty) || string.IsNullOrWhiteSpace(Name) || Content.Equals(Stream.Null);
    }
}