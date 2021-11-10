using Iis.DbLayer.Repositories;
using IIS.Core.FlightRadar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Api.FlightRadar
{
    public static class FlightRadarRegistrationModule
    {
        private const string DataReaderSectionName = "flightRadarDataReader";

        public static IServiceCollection RegisterFlightRadarServices(this IServiceCollection services, IConfiguration configuration)
        {
            var flightRadarDataReaderConfig = configuration.GetSection(DataReaderSectionName).Get<FlightRadarDataReaderConfig>();
            services.AddSingleton(flightRadarDataReaderConfig);

            services.AddTransient<IFlightRadarService, FlightRadarService<IIISUnitOfWork>>();
            services.AddHostedService<FlightRadarHistorySyncJob>();
            services.AddHostedService<FlightRadarDataImporter>();

            return services;
        }
    }
}
