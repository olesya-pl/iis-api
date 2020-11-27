using Iis.DataModel.Themes;
using Iis.Services.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.BackgroundServices
{
    public class ThemeCounterBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThemeCounterBackgroundService> _logger;

        private static bool _objectUpdateNeeded = false;
        private static bool _eventUpdateNeeded = false;
        private static bool _materialUpdateNeeded = false;
        private static bool UpdateNeeded => _objectUpdateNeeded 
            || _eventUpdateNeeded 
            || _materialUpdateNeeded;

        public static void SignalMaterialUpdateNeeded()
        {
            _materialUpdateNeeded = true;
        }
        
        public static void SignalObjectUpdateNeeded()
        {
            _objectUpdateNeeded = true;
        }

        public static void SignalEventUpdateNeeded()
        {
            _eventUpdateNeeded = true;
        }

        public ThemeCounterBackgroundService(IServiceProvider serviceProvider, ILogger<ThemeCounterBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (UpdateNeeded)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                            var typesToUpdate = GetTypesToUpdate();
                            ResetFlags();
                            var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
                            await themeService.UpdateQueryResultsAsync(stoppingToken, typesToUpdate);
                            var sleepInterval = configuration.GetValue("themesRefreshInterval", 120);
                            await Task.Delay(TimeSpan.FromSeconds(sleepInterval), stoppingToken);
                        }
                    }
                    catch (Exception e) 
                    {
                        _logger.LogError(e, $"Error occur while {nameof(ThemeCounterBackgroundService)} excution");
                        await Task.Delay(TimeSpan.FromMinutes(2));
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
        }

        private Guid[] GetTypesToUpdate()
        {
            var res = new List<Guid>();

            if (_objectUpdateNeeded)
            {
                res.Add(ThemeTypeEntity.EntityObjectId);
                res.Add(ThemeTypeEntity.EntityMapId);
            }
            if (_eventUpdateNeeded)
            {
                res.Add(ThemeTypeEntity.EntityEventId);
            }
            if (_materialUpdateNeeded)
            {
                res.Add(ThemeTypeEntity.EntityMaterialId);
            }

            return res.ToArray();
        }

        private static void ResetFlags()
        {
            _objectUpdateNeeded = false;
            _eventUpdateNeeded = false;
            _materialUpdateNeeded = false;
        }
    }
}
