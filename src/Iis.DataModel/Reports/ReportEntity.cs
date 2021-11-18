using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.DataModel.Reports
{
    public class ReportEntity : BaseEntity
    {
        public ReportEntity() { }

        public ReportEntity(ReportEntity report, Guid newId, DateTime createdAt)
        {
            if (report is null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            Id = newId;
            Title = report.Title;
            Recipient = report.Recipient;
            AccessLevel = report.AccessLevel;
            CreatedAt = createdAt;

            ReportEvents = new List<ReportEventEntity>(report.ReportEvents.Select(re => new ReportEventEntity
            {
                EventId = re.EventId,
                ReportId = newId
            }));
        }

        public string Title { get; set; }
        public string Recipient { get; set; }
        public int AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Annotation { get; set; }
        public virtual ICollection<ReportEventEntity> ReportEvents { get; set; } = new List<ReportEventEntity>();
    }
}
