using System;
using Iis.Interfaces.Metrics;
using Iis.Metrics.Constants;
using Prometheus;

namespace Iis.Metrics.Materials
{
    public class MaterialMetricsManager : IMaterialMetricsManager
    {
        private const string AppLabel = "app";
        private const string ModuleLabel = "module";
        private const string ComponentName = "iis";
        private const string SourceLabel = "source";

        private readonly ApplicationMetrics _applicationMetrics;
        private Gauge _materialsTotalCountMetric;
        private Gauge _materialsCountMetric;

        public MaterialMetricsManager(ApplicationMetrics applicationMetrics)
        {
            _applicationMetrics = applicationMetrics;
            _materialsTotalCountMetric = InitGauge(MetricNames.Materials.MaterialsTotalCount, "Count of all downloaded materials", AppLabel, ModuleLabel);
            _materialsCountMetric = InitGauge(MetricNames.Materials.MaterialsCount, "Count of downloaded materials", AppLabel, ModuleLabel, SourceLabel);
        }

        public IGauge MaterialsTotalCountMetric => _materialsTotalCountMetric.WithLabels(ComponentName, ComponentName);

        public IGauge GetMaterialsCountMetric(string source) => _materialsCountMetric.WithLabels(ComponentName, ComponentName, source);

        public void SetMetrics(MaterialMetrics materialMetrics)
        {
            MaterialsTotalCountMetric.Set(materialMetrics.TotalCount);

            foreach (var (type, count) in materialMetrics.BySourceCounts)
            {
                GetMaterialsCountMetric(type).Set(count);
            }
        }

        private Gauge InitGauge(string metricName, string help, params string[] labelNames)
        {
            var configuration = new GaugeConfiguration
            {
                LabelNames = labelNames
            };

            if (!_applicationMetrics.CreateGauge(
                metricName,
                help,
                configuration,
                out var gauge))
            {
                throw new InvalidOperationException($"'{metricName}' metric already exists!");
            }

            return gauge;
        }
    }
}