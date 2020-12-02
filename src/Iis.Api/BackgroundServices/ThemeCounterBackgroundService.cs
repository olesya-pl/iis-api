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
        private const string RefreshIntervalInSecondsParamName = "themesRefreshIntervalInSeconds";
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThemeCounterBackgroundService> _logger;
        private readonly int _refreshIntervalInSeconds;

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

            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            _refreshIntervalInSeconds = configuration.GetValue(RefreshIntervalInSecondsParamName, 100);
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
                            var typesToUpdate = GetTypesToUpdate();
                            ResetFlags();
                            var themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
                            await themeService.UpdateQueryResultsAsync(stoppingToken, typesToUpdate);
                            await Task.Delay(TimeSpan.FromSeconds(_refreshIntervalInSeconds), stoppingToken);
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
