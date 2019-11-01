using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Report.EntityFramework
{
    public class Report
    {
        public Guid Id            { get; set; }
        public string Title       { get; set; }
        public string Recipient   { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<ReportEvents> ReportEvents { get; set; } = new List<ReportEvents>();

        public Report() { }

        public Report(Report report, Guid newId, DateTime createdAt)
        {
            if (report is null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            Id        = newId;
            Title     = report.Title;
            Recipient = report.Recipient;
            CreatedAt = createdAt;

            ReportEvents = new List<ReportEvents>(report.ReportEvents.Select(re => new ReportEvents
            {
                EventId  = re.EventId,
                ReportId = newId
            }));
        }
    }
}
