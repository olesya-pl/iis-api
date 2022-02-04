using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using AutoMapper;
using Iis.RabbitMq.Helpers;
using Iis.RabbitMq.Channels;
using Iis.Messages.Materials;
using Iis.MaterialDistributor.Configurations;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;

namespace Iis.MaterialDistributor.Workers
{
    public class MaterialCoefficientsConsumer : BackgroundService
    {
        private const int RetryIntervalSeconds = 5;
        private readonly ILogger<MaterialCoefficientsConsumer> _logger;
        private readonly IMapper _mapper;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly MaterialCoefficientPublisherOptions _publisherOptions;
        private readonly MaterialCoefficientConsumerOptions _consumerOptions;
        private IConnection _connection;
        private IPublishMessageChannel<MaterialCoefficientEvaluatedEventMessage> _publishChannel;
        private IConsumeMessageChannel<MaterialProcessingCoefficientEventMessage> _consumeChannel;

        public MaterialCoefficientsConsumer(
            ILogger<MaterialCoefficientsConsumer> logger,
            IMapper mapper,
            IConnectionFactory connectionFactory,
            IServiceProvider serviceProvider,
            IOptions<MaterialCoefficientPublisherOptions> publisherOptions,
            IOptions<MaterialCoefficientConsumerOptions> consumerOptions)
        {
            _logger = logger;
            _mapper = mapper;
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _publisherOptions = publisherOptions.Value;
            _consumerOptions = consumerOptions.Value;
        }

        public override void Dispose()
        {
            if (_consumeChannel != null)
            {
                _consumeChannel.Dispose();
                _publishChannel.Dispose();
                _consumeChannel = null;
                _publishChannel = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }

            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _connectionFactory.CreateAndWaitConnection(RetryIntervalSeconds, _logger, _consumerOptions.HandlerName);

            _publishChannel = new PublishMessageChannel<MaterialCoefficientEvaluatedEventMessage>(
                _connection,
                _publisherOptions.SourceChannel);

            _consumeChannel = new ConsumeMessageChannel<MaterialProcessingCoefficientEventMessage>(
                _connection,
                _consumerOptions.SourceChannel,
                _logger);

            _consumeChannel.OnMessageReceived = ProcessMessageAsync;

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(MaterialProcessingCoefficientEventMessage message)
        {
            using var scope = _serviceProvider.CreateScope();

            var evaluator = scope.ServiceProvider.GetRequiredService<IPermanentCoefficientEvaluator>();
            var repository = scope.ServiceProvider.GetRequiredService<IPermanentCoefficientRepository>();

            var materials = _mapper.Map<MaterialInfo[]>(message.Materials);

            var coefficients = await repository.GetAsync(CancellationToken.None);
            var evaluationResult = evaluator.Evaluate(coefficients, materials);
            var materialCoefficients = _mapper.Map<MaterialCoefficient[]>(evaluationResult);

            if (materialCoefficients.Length == 0) return;

            var evaluationEvent = new MaterialCoefficientEvaluatedEventMessage
            {
                MaterialCoefficients = materialCoefficients
            };

            _publishChannel.Send(evaluationEvent);
        }
    }
}