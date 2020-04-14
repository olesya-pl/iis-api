using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

using Iis.Api.Configuration;
using IIS.Core;
using IIS.Core.Materials;

namespace Iis.Api.Modules
{
    internal static class MaterialEventRegistrationModule
    {
        private const string eventSectionName = "materialEventPublisher";
        public static IServiceCollection RegisterMaterialEventServices(this IServiceCollection services, IConfiguration configuration)
        {
            var gsmWorkerUrl = configuration.GetValue<string>("gsmWorkerUrl");
            
            var meConfig = configuration.GetSection(eventSectionName).Get<MaterialEventConfiguration>();
            
            return services
                        .AddSingleton<MaterialEventConfiguration>(serviceProvider => meConfig)
                        .AddTransient<IGsmTranscriber>(e => new GsmTranscriber(gsmWorkerUrl))
                        .AddTransient<IMaterialEventProducer, MaterialEventProducer>()
                        .AddHostedService<MaterialEventConsumer>();
        }
    }
}