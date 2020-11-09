using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.Domain.FlightRadar;
using Iis.FlightRadar.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IIS.Core.FlightRadar
{
    public class FlightRadarHistorySyncJob : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IMapper _mapper;
        private readonly IFlightRadarService _flightRadarService;
        private readonly ILogger<FlightRadarHistorySyncJob> _logger;
        private const int batchSize = 5000;

        public FlightRadarHistorySyncJob(IServiceProvider provider,
            IMapper mapper,
            IFlightRadarService flightRadarService,
            ILogger<FlightRadarHistorySyncJob> logger)
        {
            _provider = provider;
            _mapper = mapper;
            _flightRadarService = flightRadarService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var minId = await _flightRadarService.GetLastProcessedIdAsync();
                    var routes = await GetRoutesAsync(minId);

                    if (!routes.Any())
                    {
                        await Task.Delay(TimeSpan.FromMinutes(2));
                        continue;
                    }

                    await SyncRoutesAsync(routes);
                    await _flightRadarService.UpdateLastProcessedIdAsync(minId, routes.Max(p => p.Id));
                }
                catch (Exception e)
                {
                    _logger.LogError("FlightRadarHistorySyncJob. Exception={e}", e);
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            }
        }

        private async Task SyncRoutesAsync(List<Routes> routes)
        {
            var icaoGroups = routes.GroupBy(p => p.Callsign);
            var saveFlightHistoryTasks = new List<Task>();

            foreach (var icaoGroup in icaoGroups)
            {
                var history = _mapper.Map<List<FlightRadarHistory>>(icaoGroup);
                saveFlightHistoryTasks.Add(_flightRadarService.SaveFlightRadarDataAsync(icaoGroup.Key, history));
            }
            await Task.WhenAll(saveFlightHistoryTasks);
        }

        private async Task<List<Routes>> GetRoutesAsync(FlightRadarHistorySyncJobConfig minId)
        {
            using var flightContext = _provider.GetRequiredService<FlightsContext>();
            return await flightContext.Routes
                .Where(p => p.Id > minId.LatestProcessedId)
                .OrderBy(p => p.Id)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
