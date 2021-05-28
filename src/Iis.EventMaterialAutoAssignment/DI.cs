using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.EventMaterialAutoAssignment
{
    public static class DI
    {
        public static IServiceCollection RegisterEventMaterialAutoAssignment(this IServiceCollection services, IConfiguration configuration)
        {
            const string assignerSectionName = "eventMaterialsAssigner";
            const string messageHandlerSectionName = "materialMessageHandler";

            var assignerConfig = configuration.GetSection(assignerSectionName)
                                            .Get<EventMaterialAssignerConfiguration>();
            var messageHandlerConfig = configuration.GetSection(messageHandlerSectionName)
                                            .Get<MaterialMessageHandlerConfiguration>();

           return services.AddSingleton(messageHandlerConfig)
                .AddSingleton(assignerConfig)
                .AddHostedService<MaterialMessageHandler>()
                .AddHostedService<EventMaterialAssigner>();
        }
    }
}