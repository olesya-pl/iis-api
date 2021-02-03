using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using Iis.Api.Materials.Handlers;
using IIS.Core.Materials;
using Iis.Messages;
using Iis.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Iis.Api.Materials
{
    public class MaterialConsumer : BackgroundService
    {
        private const int ReConnectTimeoutSec = 5;
        private const int PrefetchCount = 5;
        private readonly ILogger<FeatureHandler> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public MaterialConsumer(
            ILogger<FeatureHandler> logger,
            IConnectionFactory connectionFactory,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            while (true)
            {
                try
                {
                    _connection = connectionFactory.CreateConnection();
                    break;
                }
                catch (BrokerUnreachableException)
                {
                    _logger.LogError($"Attempting to connect again in {ReConnectTimeoutSec} sec.");

                    Thread.Sleep(ReConnectTimeoutSec * 1000);
                }
            }

            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.QueueDeclare(
                queue: MaterialRabbitConsts.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.BasicQos(0, PrefetchCount, global: false);
            

            ConfigureConsumer(_channel, ProcessMessage);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _channel.Dispose();

            _connection.Close();
            _connection.Dispose();

            base.Dispose();
        }

        private async Task ProcessMessage(MaterialCreatedMessage message)
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

                var materialEvent = new MaterialEventMessage
                {
                    Id = message.MaterialId,
                    Source = message.Source,
                    Type = message.Type
                };

                eventProducer.SendMaterialEvent(materialEvent);
                eventProducer.SendMaterialFeatureEvent(materialEvent);

                // todo: multiple queues for different material types
                if (message.FileId.HasValue && message.Type == "cell.voice")
                {
                    eventProducer.SendMaterialAddedEventAsync(
                        new MaterialAddedEvent
                        {
                            FileId = message.FileId.Value,
                            MaterialId = message.MaterialId
                        });
                }
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