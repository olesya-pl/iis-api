using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.EventMaterialAutoAssignment
{
    public static class DI
    {
        public static IServiceCollection RegisterEventMaterialAutoAssignment(this IServiceCollection services, IConfiguration configuration)
        {
            const string assignerSectionName = "eventMaterialsAssigner";

            var assignerConfig = configuration.GetSection(assignerSectionName)
                                            .Get<EventMaterialAssignerConfiguration>();

           return services.AddSingleton(assignerConfig)
                .AddHostedService<Worker>();
        }
    }
}