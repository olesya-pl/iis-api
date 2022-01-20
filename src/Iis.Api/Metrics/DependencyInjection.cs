using Iis.Interfaces.Metrics;
using Iis.Metrics;
using Iis.Metrics.Materials;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Api.Metrics
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMetrics(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationMetrics>();

            services.AddSingleton<IMaterialMetricsManager, MaterialMetricsManager>();

            services.AddScoped<MaterialMetricsUpdater>();

            services.AddHostedService<MaterialMetricsBackgroundService>();

            return services;
        }
    }
}