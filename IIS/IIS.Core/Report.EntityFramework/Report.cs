using System;
using System.Collections.Generic;

namespace IIS.Core.Report.EntityFramework
{
    public class Report
    {
        public Guid Id            { get; set; }
        public string Title       { get; set; }
        public string Recipient   { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<ReportEvents> ReportEvents { get; set; } = new List<ReportEvents>();
    }
}
