using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Iis.Interfaces.Elastic;
using Iis.Elastic.SearchResult;
namespace Iis.Elastic
{
    public static class DI
    {
        private const string ElasticConfigurationSectionName = "elasticSearch";

        public static IServiceCollection RegisterElasticManager(this IServiceCollection services, IConfiguration configuration, out ElasticConfiguration elasticConfiguration)
        {
            elasticConfiguration = configuration.GetSection(ElasticConfigurationSectionName).Get<ElasticConfiguration>();

            if (elasticConfiguration is null) throw new InvalidOperationException($"No config was found within section '{ElasticConfigurationSectionName}'");

            services.AddSingleton<ElasticConfiguration>(elasticConfiguration);
            services.AddSingleton<ElasticLogUtils>();
            services.AddTransient<SearchResultExtractor>();
            services.AddTransient<IElasticManager, ElasticManager>();

            return services;
        }

        public static IServiceCollection RegisterElasticManager(this IServiceCollection services, IConfiguration configuration)
        {
            return services.RegisterElasticManager(configuration, out ElasticConfiguration elasticConfiguration);
        }
    }
}
