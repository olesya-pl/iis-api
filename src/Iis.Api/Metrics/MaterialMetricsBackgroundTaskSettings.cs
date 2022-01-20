using System;

namespace Iis.Api.Metrics
{
    public class MaterialMetricsBackgroundTaskSettings
    {
        public const string SectionName = "MaterialMetrics";
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan DefaultErrorTimeout = TimeSpan.FromMinutes(2);

        public TimeSpan? Timeout { get; set; }
        public TimeSpan? ErrorTimeout { get; set; }
    }
}