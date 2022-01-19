using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Workers;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class HostedServicesModule
    {
        public static IServiceCollection RegisterHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<DistributionWorker>();
            return services;
        }
    }
}