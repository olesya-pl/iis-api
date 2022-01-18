using System;
using System.Collections.Generic;
using Prometheus;

namespace Iis.Metrics
{
    public class ApplicationMetrics
    {
        private readonly Dictionary<string, Gauge> _gauges = new Dictionary<string, Gauge>();

        public Gauge GetGauge(string name)
        {
            if (_gauges.TryGetValue(name, out var gauge)) return gauge;

            throw new ArgumentException("Unknown gauge name");
        }

        public bool CreateGauge(string name, string help, GaugeConfiguration configuration, out Gauge gauge)
        {
            gauge = Prometheus.Metrics.CreateGauge(name, help, configuration);

            return _gauges.TryAdd(name, gauge);
        }
    }
}