using System;

namespace Iis.Services.Contracts.Dtos
{
    public class FileIdDto
    {
        public Guid Id { get; set; }

        public bool IsDuplicate { get; set; }
    }
}