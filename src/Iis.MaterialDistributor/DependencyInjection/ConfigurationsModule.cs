using Iis.MaterialDistributor.Contracts.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class ConfigurationsModule
    {
        public static IServiceCollection RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MaterialCoefficientPublisherOptions>(configuration.GetSection(MaterialCoefficientPublisherOptions.SectionName));
            services.Configure<MaterialCoefficientConsumerOptions>(configuration.GetSection(MaterialCoefficientConsumerOptions.SectionName));

            return services;
        }
    }
}