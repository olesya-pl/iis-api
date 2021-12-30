using System.Collections.Generic;

namespace Iis.Interfaces.Metrics
{
    public class MaterialMetrics
    {
        public long TotalCount { get; set; }
        public IReadOnlyDictionary<string, long> BySourceCounts { get; set; }
    }
}