using Prometheus;

namespace Iis.Interfaces.Metrics
{
    public interface IMaterialMetricsManager
    {
        IGauge MaterialsTotalCountMetric { get; }

        IGauge GetMaterialsCountMetric(string source);
        void SetMetrics(MaterialMetrics materialMetrics);
    }
}