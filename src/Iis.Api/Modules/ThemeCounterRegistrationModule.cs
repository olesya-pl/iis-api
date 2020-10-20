using Microsoft.Extensions.DependencyInjection;
using Iis.Api.BackgroundServices;

namespace Iis.Api.Modules
{
    internal static class ThemeCounterRegistrationModule
    {
        public static IServiceCollection RegisterThemeCounterService(this IServiceCollection services)
        {
#if DEBUG
            return services;
#else
            return services.AddHostedService<ThemeCounterBackgroundService>();
#endif
        }
    }
}