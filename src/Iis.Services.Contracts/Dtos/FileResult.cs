using System;

namespace Iis.Services.Contracts.Dtos
{
    public class FileResult
    {
        public Guid Id { get; set; }
        public bool IsDuplicate { get; set; }
        public Guid FileHash { get; set; }
    }
}