using System;
using System.Collections.Generic;

namespace Iis.Services.Contracts.Dtos
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Recipient { get; set; }
        public int AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Annotation { get; set; }
        public IEnumerable<Guid> ReportEventIds { get; set; }
    }
}
