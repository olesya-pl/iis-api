using System;

namespace Iis.Services.Contracts.Dtos
{
    public class FileResult
    {
        public Guid Id;
        public bool IsDuplicate;
        public Guid FileHash;
    }
}