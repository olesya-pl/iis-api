using System;

namespace IIS.Core.Analytics.EntityFramework
{
    public class AnalyticsQueryIndicator
    {
        public Guid Id { get; set; }
        public Guid QueryId { get; set; }
        public Guid IndicatorId  { get; set; }
        public string Title { get; set; }
        public int SortOrder { get; set; }

        public virtual AnalyticsIndicator Indicator { get; set; }
        public virtual AnalyticsQuery Query { get; set; }
    }
}
