using System;
using System.Collections.Generic;
using MediatR;

namespace Iis.Events.Reports
{
    public abstract class ReportEvent : INotification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Recipient { get; set; }
        public int AccessLevel { get; set; }
        public string Annotation { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<Guid> ReportEventIds { get; set; }
    }
}
