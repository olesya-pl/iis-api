using Iis.Interfaces.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Iis.Api.Metrics
{
    internal static class HostExtensions
    {
        public static IHost InitAppMetrics(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var materialMetricsManager = scope.ServiceProvider.GetRequiredService<IMaterialMetricsManager>();

            materialMetricsManager.SetDefaultValues();

            return host;
        }
    }
}