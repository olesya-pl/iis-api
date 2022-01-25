using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Workers;
using Iis.MaterialDistributor.Configurations;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class HostedServicesModule
    {
        private const string DictributionConfigurationSectionName = "distribution";

        public static IServiceCollection RegisterHostedServices(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection(DictributionConfigurationSectionName).Get<DistributionConfig>();

            services.AddSingleton<DistributionConfig>(config);

            services.AddHostedService<DistributionWorker>();
            services.AddHostedService<MaterialCoefficientsConsumer>();

            return services;
        }
    }
}