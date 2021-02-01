using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Iis.Api.Configuration;
using IIS.Core;
using IIS.Core.Materials;
using IIS.Core.Materials.Handlers;
using IIS.Core.Materials.Handlers.Configurations;
using Iis.Api.Materials;
using Iis.Api.RabbitConsumers;

namespace Iis.Api.Modules
{
    internal static class MaterialEventRegistrationModule
    {
        private const string eventSectionName = "materialEventPublisher";
        private const string assignerSectionName = "operatorAssigner";
        private const string featureHandlerSectionName = "featureHandler";
        private const string elasticSaverSectionName = "elasticSaver";
        public static IServiceCollection RegisterMaterialEventServices(this IServiceCollection services, IConfiguration configuration)
        {
            var gsmWorkerUrl = configuration.GetValue<string>("gsmWorkerUrl");

            var meConfig = configuration.GetSection(eventSectionName)
                                            .Get<MaterialEventConfiguration>();
            var assignerConfig = configuration.GetSection(assignerSectionName)
                                            .Get<MaterialOperatorAssignerConfiguration>();
            var featureHandlerConfig = configuration.GetSection(featureHandlerSectionName)
                                                    .Get<FeatureHandlerConfig>();
            var elasticSaver = configuration.GetSection(elasticSaverSectionName)
                                                    .Get<CreatedMaterialElasticSaverConfiguration>();

            return services
                        .AddSingleton<MaterialEventConfiguration>(serviceProvider => meConfig)
                        .AddSingleton(assignerConfig)
                        .AddSingleton(featureHandlerConfig)
                        .AddSingleton(elasticSaver)
                        .AddTransient<IGsmTranscriber>(e => new GsmTranscriber(gsmWorkerUrl))
                        .AddTransient<IMaterialEventProducer, MaterialEventProducer>()
                        .AddHostedService<MaterialOperatorAssigner>()
                        .AddHostedService<CreatedMaterialElasticSaver>()
                        .AddHostedService<FeatureHandler>()
                        .AddHostedService<MaterialConsumer>();
        }
    }
}