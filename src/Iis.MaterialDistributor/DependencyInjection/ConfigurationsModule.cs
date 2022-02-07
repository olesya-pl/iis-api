using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Configurations;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class ConfigurationsModule
    {
        public static IServiceCollection RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MaterialCoefficientPublisherOptions>(configuration.GetSection(MaterialCoefficientPublisherOptions.SectionName));
            services.Configure<MaterialCoefficientConsumerOptions>(configuration.GetSection(MaterialCoefficientConsumerOptions.SectionName));
            services.Configure<MaterialNextAssignedConsumerConfig>(configuration.GetSection(MaterialNextAssignedConsumerConfig.SectionName));

            return services;
        }
    }
}