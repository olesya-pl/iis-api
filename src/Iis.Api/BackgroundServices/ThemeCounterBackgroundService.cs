using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Iis.ThemeManagement;
using Iis.DbLayer.Repositories;
using Microsoft.Extensions.Logging;

namespace Iis.Api.BackgroundServices
{
    public class ThemeCounterBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThemeCounterBackgroundService> _logger;

        public ThemeCounterBackgroundService(IServiceProvider serviceProvider, ILogger<ThemeCounterBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var themeService = scope.ServiceProvider.GetRequiredService<ThemeService<IIISUnitOfWork>>();
                        await themeService.UpdateQueryResultsAsync(stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception e) 
                {
                    _logger.LogError(e, $"Error occur while {nameof(ThemeCounterBackgroundService)} excution");
                    await Task.Delay(TimeSpan.FromMinutes(2));
                }
            }
        }
    }
}
