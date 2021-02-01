using Iis.DbLayer.Repositories;
using IIS.Core.FlightRadar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Api.FlightRadar
{
    public static class FlightRadarRegistrationModule
    {
        public static IServiceCollection RegisterFlightRadarServices(this IServiceCollection services, IConfiguration configuration)
        {
            var flightRadarDataReaderConfig = configuration.GetSection("flightRadarDataReader").Get<FlightRadarDataReaderConfig>();
            services.AddSingleton(flightRadarDataReaderConfig);

            services.AddTransient<IFlightRadarService, FlightRadarService<IIISUnitOfWork>>();
            // services.AddHostedService<FlightRadarHistorySyncJob>();
            // services.AddHostedService<FlightRadarDataImporter>();

            return services;
        }
    }
}
