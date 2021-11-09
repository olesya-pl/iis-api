using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.EventMaterialAutoAssignment
{
    public static class DI
    {
        public static IServiceCollection RegisterEventMaterialAutoAssignment(this IServiceCollection services, IConfiguration configuration)
        {
            const string AssignerSectionName = "eventMaterialsAssigner";
            const string MessageHandlerSectionName = "materialMessageHandler";

            var assignerConfig = configuration.GetSection(AssignerSectionName)
                                            .Get<EventMaterialAssignerConfiguration>();
            var messageHandlerConfig = configuration.GetSection(MessageHandlerSectionName)
                                            .Get<MaterialMessageHandlerConfiguration>();

           return services.AddSingleton(messageHandlerConfig)
                .AddSingleton(assignerConfig)
                .AddHostedService<MaterialMessageHandler>()
                .AddHostedService<EventMaterialAssigner>();
        }
    }
}