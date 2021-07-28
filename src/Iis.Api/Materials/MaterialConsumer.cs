using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using Iis.Api.Materials.Handlers;
using IIS.Core.Materials;
using Iis.Messages;
using Iis.Messages.Materials;
using Iis.Utility;
using Iis.RabbitMq.Helpers;
using Iis.RabbitMq.Channels;
using Iis.Services.Contracts.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Iis.Api.Materials
{
    public class MaterialConsumer : BackgroundService
    {
        private const int RetryIntervalSec = 5;
        private const int PrefetchCount = 5;
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

                eventProducer.SaveMaterialToElastic(message.MaterialId);

                if (!message.ParentId.HasValue)
                {
                    eventProducer.SendAvailableForOperatorEvent(message.MaterialId);
                }

                var materialEvent = new MaterialProcessingEventMessage
                {
                    Id = message.MaterialId,
                    Source = message.Source,
                    Type = message.Type
                };

                eventProducer.SendMaterialProcessingEvent(materialEvent);

                return Task.CompletedTask;
            }
        }

        private void ConfigureConsumer(IModel channel, Func<MaterialCreatedMessage, Task> handler)
        {
            var channelConsumer = new AsyncEventingBasicConsumer(channel);

            channelConsumer.Received += async (sender, args) =>
            {
                try
                {
                    if (args.Body.LongLength == 0)
                    {
                        throw new InvalidOperationException("We received empty message.");
                    }

                    await handler(args.Body.FromBytes<MaterialCreatedMessage>());

                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception {ex} during receiving message.", ex);

                    channel.BasicReject(args.DeliveryTag, false);
                }
            };

            channel.BasicConsume(queue: MaterialRabbitConsts.QueueName, false, consumer: channelConsumer);
        }
    }
}