using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using RabbitMQ.Client;
using Iis.RabbitMq.Helpers;
using Iis.RabbitMq.Channels;
using Iis.Messages.Materials;
using Iis.MaterialDistributor.Configurations;
using Iis.MaterialDistributor.Contracts.Events;

namespace Iis.MaterialDistributor.Workers
{
    public class DistributionWorker : BackgroundService
    {
        private const string ApplicationName = "Iis.MaterialDistributor.DistributionWorker";
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IConnectionFactory _connectionFactory;
        private readonly DistributionConfig _configuration;
        private readonly MaterialNextAssignedConsumerConfig _consumerOptions;
        private IConnection _connection;
        private IRPCConsumeMessageChannel<MaterialNextAssignedMessage, MaterialNextAssignedMessage> _rpcConsumeChannel;

        public DistributionWorker(
            ILogger<DistributionWorker> logger,
            IMediator mediator,
            IConnectionFactory connectionFactory,
            DistributionConfig configuration,
            IOptions<MaterialNextAssignedConsumerConfig> consumerOptions)
        {
            _logger = logger;
            _mediator = mediator;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _consumerOptions = consumerOptions.Value;
        }

        public override void Dispose()
        {
            if (_rpcConsumeChannel != null)
            {
                _rpcConsumeChannel.Dispose();
                _rpcConsumeChannel = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }

            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _connectionFactory.CreateAndWaitConnection(logger: _logger, clientName: ApplicationName);

            _rpcConsumeChannel = MessageChannels.CreateRPCConsumer<MaterialNextAssignedMessage>(_connection, _consumerOptions.SourceChannel, _logger);

            _rpcConsumeChannel.SetOnMessageReceived(ProcessMessage);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _mediator.Publish(new ReadAllMaterialsEvent(), stoppingToken);

                await Task.Delay(_configuration.RefreshMaterialInterval, stoppingToken);
            }
        }

        private async Task<MaterialNextAssignedMessage> ProcessMessage(MaterialNextAssignedMessage message)
        {
            var materialId = await _mediator.Send(new GetNextAssignedMaterial { UserId = message.UserId });

            return new MaterialNextAssignedMessage { UserId = message.UserId, MaterialId = materialId };
        }
    }
}