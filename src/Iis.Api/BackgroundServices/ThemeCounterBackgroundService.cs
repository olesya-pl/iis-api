using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Iis.ThemeManagement;
using Iis.DbLayer.Repositories;
using Newtonsoft.Json.Bson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Iis.Api.BackgroundServices
{
    public class ThemeCounterBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThemeCounterBackgroundService> _logger;
        private static bool _updateNeeded = false;

        public static void SignalThemeUpdateNeeded()
        {
            _updateNeeded = true;
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
                if (_updateNeeded)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                            _updateNeeded = false;
                            var themeService = scope.ServiceProvider.GetRequiredService<ThemeService<IIISUnitOfWork>>();
                            await themeService.UpdateQueryResultsAsync(stoppingToken);
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
    }
}
