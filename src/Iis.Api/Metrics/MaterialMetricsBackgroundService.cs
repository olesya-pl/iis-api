using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Iis.Api.Metrics
{
    public class MaterialMetricsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaterialMetricsBackgroundService> _logger;
        private readonly MaterialMetricsBackgroundTaskSettings _settings;

        public MaterialMetricsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MaterialMetricsBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = configuration
                .GetSection(BackgroundServiceSettings.SectionName)
                .GetSection(MaterialMetricsBackgroundTaskSettings.SectionName)
                .Get<MaterialMetricsBackgroundTaskSettings>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var materialMetricsUpdater = scope.ServiceProvider.GetRequiredService<MaterialMetricsUpdater>();

                    await materialMetricsUpdater.UpdateAsync(stoppingToken);
                    await Task.Delay(_settings.Timeout ?? MaterialMetricsBackgroundTaskSettings.DefaultTimeout, stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Error occured while {nameof(MaterialMetricsBackgroundService)} excution");
                    await Task.Delay(_settings.ErrorTimeout ?? MaterialMetricsBackgroundTaskSettings.DefaultErrorTimeout, stoppingToken);
                }
            }
        }
    }
}