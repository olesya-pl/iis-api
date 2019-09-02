using System;

namespace IIS.Core.GraphQL.Files
{
    public class FileInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public bool IsTemporary { get; set; }
    }
}
