using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;

namespace Iis.MaterialDistributor.Workers
{
    public class DistributionWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public DistributionWorker(
            ILogger<DistributionWorker> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mediator.Publish(new ReadAllMaterialsEvent { HourOffset = 2 }, stoppingToken);

            /*
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Ping");

                await Task.Delay(System.TimeSpan.FromSeconds(5), stoppingToken);
            }
            */
        }
    }
}