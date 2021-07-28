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
        private const string eventSectionName = "materialEventPublisher";
        private const string assignerSectionName = "operatorAssigner";
        private const string featureHandlerSectionName = "featureHandler";
        private const string elasticSaverSectionName = "elasticSaver";
        private const string MaterialConsumerSectionName = "materialConsumer";
        public static IServiceCollection RegisterMaterialEventServices(this IServiceCollection services, IConfiguration configuration)
        {
            var meConfig = configuration.GetSection(eventSectionName)
                                            .Get<MaterialEventConfiguration>();
            var assignerConfig = configuration.GetSection(assignerSectionName)
                                            .Get<MaterialOperatorAssignerConfiguration>();
            var featureHandlerConfig = configuration.GetSection(featureHandlerSectionName)
                                                    .Get<FeatureHandlerConfig>();
            var elasticSaver = configuration.GetSection(elasticSaverSectionName)
                                                    .Get<MaterialElasticSaverConfiguration>();
            var materialConsumerConfig = configuration.GetSection(MaterialConsumerSectionName)
                                                    .Get<MaterialConsumerConfiguration>();

            return services
                        .AddSingleton<MaterialEventConfiguration>(serviceProvider => meConfig)
                        .AddSingleton<MaterialConsumerConfiguration>(materialConsumerConfig)
                        .AddSingleton(assignerConfig)
                        .AddSingleton(featureHandlerConfig)
                        .AddSingleton(elasticSaver)
                        .AddTransient<IMaterialEventProducer, MaterialEventProducer>()
                        .AddHostedService<MaterialOperatorConsumer>()
                        .AddHostedService<MaterialElasticConsumer>()
                        .AddHostedService<FeatureHandler>()
                        .AddHostedService<MaterialConsumer>();
        }
    }
}