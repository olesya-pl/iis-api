using IIS.Core.Ontology.EntityFramework.Context;
using System;

namespace IIS.Core.Report.EntityFramework
{
    public class ReportEvents
    {
        public Guid   ReportId { get; set; }
        public Guid   EventId  { get; set; }
        public Report Report   { get; set; }
        public Node   Node     { get; set; }
    }
}
