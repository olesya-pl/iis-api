using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Iis.ThemeManagement;
using Iis.DbLayer.Repositories;

namespace Iis.Api.BackgroundServices
{
    public class ThemeCounterBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ThemeCounterBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var themeService = scope.ServiceProvider.GetRequiredService<ThemeService<IIISUnitOfWork>>();
                    await themeService.UpdateQueryResultsAsync(stoppingToken);
                } 
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
