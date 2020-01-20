using System;

namespace Iis.DataModel.Reports
{
    public class ReportEventEntity
    {
        public Guid ReportId { get; set; }
        public ReportEntity Report { get; set; }

        public Guid EventId { get; set; }
        public NodeEntity Node { get; set; }
    }
}
