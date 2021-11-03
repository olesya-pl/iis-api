using Iis.CoordinatesEventHandler.Configurations;
using Iis.CoordinatesEventHandler.Handlers;
using Iis.CoordinatesEventHandler.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Iis.CoordinatesEventHandler.DependencyInjection
{
    public static class RegistrationModule
    {
        private const string HandlerSectionName = "coordinatesMessageHandler";
        public static IServiceCollection RegisterCoordinatesMessageHandler(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection(HandlerSectionName)
                            .Get<MessageHandlerConfiguration>();

            services.AddSingleton(config);
            services.AddTransient<ICoordinatesProcessorsFactory, CoordinatesProcessorsFactory>();
            services.AddHostedService<MessageHandler>();

            return services;
        }
    }
}