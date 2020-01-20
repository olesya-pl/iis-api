using System;

namespace Iis.DataModel.Analytics
{
    public class AnalyticQueryIndicatorEntity
    {
        public Guid Id { get; set; }
        public Guid QueryId { get; set; }
        public Guid IndicatorId  { get; set; }
        public string Title { get; set; }
        public int SortOrder { get; set; }

        public virtual AnalyticIndicatorEntity Indicator { get; set; }
        public virtual AnalyticQueryEntity Query { get; set; }
    }
}
