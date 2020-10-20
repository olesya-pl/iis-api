using Microsoft.Extensions.DependencyInjection;
using Iis.Api.BackgroundServices;
using IIS.Core.FlightRadar;
using Iis.DbLayer.Repositories;

namespace Iis.Api.Modules
{
    internal static class FlightRadarRegistrationModule
    {
        public static IServiceCollection RegisterFlightRadarServices(this IServiceCollection services)
        {
            services.AddTransient<IFlightRadarService, FlightRadarService<IIISUnitOfWork>>();

#if DEBUG
            return services;
#else
            return services.AddHostedService<FlightRadarHistorySyncJob>();
#endif
        }
    }
}