using System;

namespace IIS.Core.Files
{
    public class FileId
    {
        public Guid Id { get; set; }

        public bool IsDuplicate { get; set; }
    }
}