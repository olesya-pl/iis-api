using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IIS.Core.Materials;
using IIS.Core.Materials.Handlers.Configurations;
using Iis.Api.Materials;
using Iis.Api.Materials.Handlers;
using Iis.Services.Contracts.Configurations;

namespace Iis.Api.Modules
{
    internal static class MaterialEventRegistrationModule
    {
        private const string EventSectionName = "materialEventPublisher";
        private const string AssignerSectionName = "operatorAssigner";
        private const string FeatureHandlerSectionName = "featureHandler";
        private const string ElasticSaverSectionName = "elasticSaver";
        private const string MaterialConsumerSectionName = "materialConsumer";
        private const string MaterialCoefficientsConsumerSectionName = "materialCoefficientsConsumer";

        public static IServiceCollection RegisterMaterialEventServices(this IServiceCollection services, IConfiguration configuration)
        {
            var meConfig = configuration.GetSection(EventSectionName)
                                            .Get<MaterialEventConfiguration>();
            var assignerConfig = configuration.GetSection(AssignerSectionName)
                                            .Get<MaterialOperatorAssignerConfiguration>();
            var featureHandlerConfig = configuration.GetSection(FeatureHandlerSectionName)
                                                    .Get<FeatureHandlerConfig>();
            var elasticSaver = configuration.GetSection(ElasticSaverSectionName)
                                                    .Get<MaterialElasticSaverConfiguration>();
            var materialConsumerConfig = configuration.GetSection(MaterialConsumerSectionName)
                                                    .Get<MaterialConsumerConfiguration>();

            services.Configure<MaterialCoefficientsConsumerConfiguration>(configuration.GetSection(MaterialCoefficientsConsumerSectionName));

            return services
                        .AddSingleton<MaterialEventConfiguration>(serviceProvider => meConfig)
                        .AddSingleton<MaterialConsumerConfiguration>(materialConsumerConfig)
                        .AddSingleton(assignerConfig)
                        .AddSingleton(featureHandlerConfig)
                        .AddSingleton(elasticSaver)
                        .AddTransient<IMaterialEventProducer, MaterialEventProducer>()
                        .AddHostedService<MaterialOperatorDistributor>()
                        .AddHostedService<MaterialElasticConsumer>()
                        .AddHostedService<FeatureHandler>()
                        .AddHostedService<MaterialConsumer>()
                        .AddHostedService<MaterialCoefficientsConsumer>();
        }
    }
}