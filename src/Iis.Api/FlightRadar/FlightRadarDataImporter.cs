using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Iis.FlightRadar.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iis.Api.FlightRadar
{
    public class FlightRadarDataImporter : BackgroundService
    {
        #region csv mapping

        private class AirportMap : ClassMap<Airports>
        {
            public AirportMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.Icao).Name("icao");
                Map(m => m.Iata).Name("iata");
                Map(m => m.Name).Name("name");
                Map(m => m.Country).Name("country");
                Map(m => m.CountryCode).Name("countryCode");
                Map(m => m.CountryCodeLong).Name("countryCodeLong");
                Map(m => m.City).Name("city");
                Map(m => m.Longitude).Name("longitude");
                Map(m => m.Latitude).Name("latitude");
                Map(m => m.Altitude).Name("altitude");
                Map(m => m.Website).Name("website");
                Map(m => m.CreatedAt).Name("createdAt");
                Map(m => m.UpdatedAt).Name("updatedAt");
            }
        }

        private class OperatorMap : ClassMap<Operators>
        {
            public OperatorMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.Icao).Name("icao");
                Map(m => m.Iata).Name("iata");
                Map(m => m.Name).Name("name");
                Map(m => m.ShortName).Name("shortName");
                Map(m => m.Country).Name("country");
                Map(m => m.About).Name("about");
                Map(m => m.CreatedAt).Name("createdAt");
                Map(m => m.UpdatedAt).Name("updatedAt");
            }
        }

        private class AircraftMap : ClassMap<Aircraft>
        {
            public AircraftMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.RegistrationNumber).Name("registration_number");
                Map(m => m.Icao).Name("icao");
                Map(m => m.Model).Name("model");
                Map(m => m.DetailedModel).Name("detailedModel");
                Map(m => m.Photo).Name("photo");
                Map(m => m.Type).Name("type");
                Map(m => m.OwnerId).Name("ownerId");
                Map(m => m.CreatedAt).Name("createdAt");
                Map(m => m.UpdatedAt).Name("updatedAt");
            }
        }

        private class FlightMap : ClassMap<Flights>
        {
            public FlightMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.FlightNo).Name("flightNo");
                Map(m => m.ScheduledDepartureAt).Name("scheduledDepartureAt");
                Map(m => m.ScheduledArrivalAt).Name("scheduledArrivalAt");
                Map(m => m.RealDepartureAt).Name("realDepartureAt");
                Map(m => m.RealArrivalAt).Name("realArrivalAt");
                Map(m => m.ExternalId).Name("externalId");
                Map(m => m.Meta).Name("meta");
                Map(m => m.ArrivalAirportId).Name("arrivalAirportId");
                Map(m => m.DepartureAirportId).Name("departureAirportId");
                Map(m => m.PlaneId).Name("planeId");
                Map(m => m.CreatedAt).Name("createdAt");
                Map(m => m.UpdatedAt).Name("updatedAt");
            }
        }

        private class RouteMap : ClassMap<Routes>
        {
            public RouteMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.Callsign).Name("callsign");
                Map(m => m.Latitude).Name("latitude");
                Map(m => m.Longitude).Name("longitude");
                Map(m => m.Altitude).Name("altitude");
                Map(m => m.Track).Name("track");
                Map(m => m.Speed).Name("speed");
                Map(m => m.TimeNow).Name("timeNow");
                Map(m => m.SquawkCode).Name("squawk_code");
                Map(m => m.FlightId).Name("flightId");
            }
        }

        #endregion

        private const string AirportsFileName = "downAirport.csv";
        private const string OperatorsFileName = "downOperator.csv";
        private const string AircraftsFileName = "downAircraft.csv";
        private const string FlightsFileName = "downFlight.csv";
        private const string RoutesFileName = "downRoutes.csv";
        private readonly IServiceProvider _provider;
        private readonly FlightRadarDataReaderConfig _config;
        private readonly ILogger<FlightRadarDataImporter> _logger;

        public FlightRadarDataImporter(IServiceProvider provider,
            FlightRadarDataReaderConfig config,
            ILogger<FlightRadarDataImporter> logger)
        {
            _provider = provider;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(_config.DataFolder))
                {
                    await Task.Delay(TimeSpan.FromMinutes(2));
                    continue;
                }
                var sourcesPaths = Directory.GetDirectories(_config.DataFolder, "*", SearchOption.AllDirectories);
                foreach (var path in sourcesPaths)
                {
                    try
                    {
                        await Load<Operators, OperatorMap>(path, OperatorsFileName, ImportOperators);                        
                        await Load<Airports, AirportMap>(path, AirportsFileName, ImportAirports);
                        await Load<Aircraft, AircraftMap>(path, AircraftsFileName, ImportAircrafts);
                        await Load<Flights, FlightMap>(path, FlightsFileName, ImportFlights);
                        await Load<Routes, RouteMap>(path, RoutesFileName, ImportRoutes);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Exception while importing data to flightradar db", e);
                    }
                    
                }
                await Task.Delay(TimeSpan.FromMinutes(2));
            }            
        }

        private async Task ImportRoutes(IEnumerable<Routes> routes)
        {
            const int batchSize = 25000;
            var count = 0;
            var toInsert = new List<Routes>(batchSize);
            
            foreach (var route in routes)
            {
                toInsert.Add(route);
                count++;
                if (count == batchSize)
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var flightContext = scope.ServiceProvider.GetRequiredService<FlightsContext>();
                        await flightContext.Routes.AddRangeAsync(toInsert);
                        await flightContext.SaveChangesAsync();
                    }
                    
                    toInsert.Clear();
                    count = 0;
                }
            }
            if (toInsert.Any())
            {
                using (var scope = _provider.CreateScope())
                {
                    var flightContext = scope.ServiceProvider.GetRequiredService<FlightsContext>();
                    await flightContext.Routes.AddRangeAsync(toInsert);
                    await flightContext.SaveChangesAsync();
                }                    
            }
        }

        private async Task ImportFlights(IEnumerable<Flights> flights)
        {
            const int batchSize = 25000;
            var count = 0;
            var toInsert = new List<Flights>(batchSize);
            using (var flightContext = _provider.GetRequiredService<FlightsContext>())
            {
                foreach (var flight in flights)
                {
                    var row = GetFlightWithMeta(flight);

                    toInsert.Add(row);
                    count++;
                    if (count == batchSize)
                    {
                        await flightContext.Flights.AddRangeAsync(toInsert);
                        await flightContext.SaveChangesAsync();
                        toInsert.Clear();
                        count = 0;
                    }
                }
                if (toInsert.Any())
                {
                    await flightContext.Flights.AddRangeAsync(toInsert);
                    await flightContext.SaveChangesAsync();
                }
            }
        }

        private static Flights GetFlightWithMeta(Flights flight)
        {
            if (string.IsNullOrEmpty(flight.Meta))
            {
                flight.Meta = null;
                return flight;
            }
            try
            {
                var _ = JObject.Parse(flight.Meta);
            }
            catch (JsonReaderException)
            {
                flight.Meta = null;
            }
            return flight;
        }

        private async Task ImportAircrafts(IEnumerable<Aircraft> items)
        {
            var aircrafts = items.ToArray();
            if (aircrafts.Any())
            {
                using var flightContext = _provider.GetRequiredService<FlightsContext>();
                await flightContext.Aircraft.AddRangeAsync(aircrafts);
                await flightContext.SaveChangesAsync();
            }
        }

        private async Task ImportAirports(IEnumerable<Airports> items)
        {
            var airports = items.ToArray();
            if (airports.Any())
            {
                using var flightContext = _provider.GetRequiredService<FlightsContext>();
                await flightContext.Airports.AddRangeAsync(airports);
                await flightContext.SaveChangesAsync();
            }
        }

        private async Task ImportOperators(IEnumerable<Operators> input)
        {
            var operators = input.ToArray();
            if (operators.Any())
            {
                using var flightContext = _provider.GetRequiredService<FlightsContext>();
                operators = operators.Select(row =>
                {
                    if (string.IsNullOrEmpty(row.About))
                    {
                        row.About = null;
                        return row;
                    }
                    try
                    {
                        var _ = JObject.Parse(row.About);
                    }
                    catch (JsonReaderException)
                    {
                        row.About = null;
                    }

                    return row;
                }).ToArray();
                await flightContext.Operators.AddRangeAsync(operators);
                await flightContext.SaveChangesAsync();
            }
        }

        private async Task Load<TEntity, TConfig>(
            string path, 
            string fileName,
            Func<IEnumerable<TEntity>, Task> syncAction) where TConfig : ClassMap
        {
            var fullFileName = Path.Combine(path, fileName);
            if (!File.Exists(fullFileName))
            {
                _logger.LogInformation($"FlightRadarDataReader. File not found {fullFileName}");
                return;
            }

            try
            {
                _logger.LogInformation($"FlightRadarDataReader. Start importing file {fullFileName}");
                using (var reader = new StreamReader(fullFileName))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<TConfig>();
                    var items = csv.GetRecords<TEntity>();
                    await syncAction(items);
                }
                _logger.LogInformation($"FlightRadarDataReader. Done importing file {fullFileName}");
            }
            catch (Exception e)
            {
                _logger.LogError("FileDataReader. Error while importing file. Exception={e}", e);
                TryMoveFile(path, fileName, fullFileName, _config.ErrorFolder);
                return;
            }
            TryMoveFile(path, fileName, fullFileName, _config.ProcessedFolder);
        }

        private void TryMoveFile(string path, string fileName, string fullFileName, string destinationDir)
        {
            try
            {
                if (!Directory.Exists(_config.DataFolder))
                {
                    _logger.LogInformation($"FlightRadarDataReader. Moving file. Directory does not exist {_config.DataFolder}");
                    return;
                }

                _logger.LogInformation($"FlightRadarDataReader. Start moving file {fullFileName}");
                var subdirectory = path.Replace(_config.DataFolder, string.Empty, StringComparison.Ordinal).TrimStart('\\', '/');
                var destinationDirectory = Path.Combine(destinationDir, subdirectory);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                var destination = Path.Combine(destinationDirectory, fileName);
                File.Move(fullFileName, destination, true);
                _logger.LogInformation($"FlightRadarDataReader. Done moving file {fullFileName} to {destination}");
            }
            catch (Exception e)
            {
                _logger.LogError("FlightRadarDataReader. Error while moving file. Exception={e}", e);
            }
        }
    }
}
