using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using Iis.MaterialDistributor.Configurations;
using Iis.MaterialDistributor.Contracts.Events;

namespace Iis.MaterialDistributor.Workers
{
    public class DistributionWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly DistributionConfig _configuration;

        public DistributionWorker(
            ILogger<DistributionWorker> logger,
            IMediator mediator,
            DistributionConfig configuration)
        {
            _logger = logger;
            _mediator = mediator;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _mediator.Publish(new ReadAllMaterialsEvent(), stoppingToken);

                await Task.Delay(_configuration.RefreshMaterialInterval, stoppingToken);
            }
        }
    }
}