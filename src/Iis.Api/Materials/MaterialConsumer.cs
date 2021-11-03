using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using IIS.Core.Materials;
using Iis.Messages.Materials;
using Iis.RabbitMq.Helpers;
using Iis.RabbitMq.Channels;
using Iis.Services.Contracts.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Iis.Api.Materials
{
    public class MaterialConsumer : BackgroundService
    {
        private const int RetryIntervalSec = 5;
        private readonly ILogger<MaterialConsumer> _logger;
        private IConnection _connection;
        private IConnectionFactory _connectionFactory;
        private MaterialConsumerConfiguration _configuration;
        private IConsumeMessageChannel<MaterialCreatedMessage> _consumeChannel;
        private readonly IServiceProvider _serviceProvider;

        public MaterialConsumer(
            ILogger<MaterialConsumer> logger,
            IConnectionFactory connectionFactory,
            MaterialConsumerConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _connectionFactory.CreateAndWaitConnection(RetryIntervalSec, _logger, _configuration.HandlerName);

            _consumeChannel = new ConsumeMessageChannel<MaterialCreatedMessage>(
                _connection,
                _configuration.SourceChannel,
                _logger
            );

            _consumeChannel.OnMessageReceived = ProcessMessage;

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if(_consumeChannel != null)
            {
                _consumeChannel.Dispose();
                _consumeChannel = null;
            }

            if(_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }

            base.Dispose();
        }

        private Task ProcessMessage(MaterialCreatedMessage message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventProducer = scope.ServiceProvider.GetService<IMaterialEventProducer>();

                ThemeCounterBackgroundService.SignalMaterialUpdateNeeded();

                var materialEvent = new MaterialProcessingEventMessage
                {
                    Id = message.MaterialId,
                    Source = message.Source,
                    Type = message.Type
                };

                eventProducer.SendMaterialToElastic(materialEvent);

                if (!message.ParentId.HasValue)
                {
                    eventProducer.SendAvailableForOperatorEvent(message.MaterialId);
                }

                return Task.CompletedTask;
            }
        }
    }
}