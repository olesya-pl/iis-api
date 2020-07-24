using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
//using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using Iis.Api.Configuration;
//using IIS.Core.Materials;
using IIS.Core.Materials.Handlers.Configurations;

namespace IIS.Core.Materials.Handlers
{
    public class FeatureHandler : BackgroundService
    {
        private const bool exclusiveQueue = false;
        private const bool durableQueue = true;
        private const bool autoDeleteQueue = false;
        private const int ReConnectTimeoutSec = 5;
        private readonly ILogger<FeatureHandler> _logger;
        private readonly FeatureHandlerConfig _сonfig;
        private IConnection _connection;
        private IModel _channel;
        private readonly JsonSerializerOptions options;
        public FeatureHandler(ILogger<FeatureHandler> logger,
            IConnectionFactory connectionFactory,
            FeatureHandlerConfig configuration)
        {
            _logger = logger;
            _сonfig = configuration;

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
            
            options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConfigureExchange(_channel, _сonfig.SourceChannel);

            ConfigureQueue(_channel, _сonfig.SourceChannel);

            ConfigureConsumer(_channel, _сonfig.SourceChannel, ProcessMessage);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _channel.Dispose();

            _connection.Close();
            _connection.Dispose();

            _channel = null;
            _connection = null;

            base.Dispose();
        }

        private Task ProcessMessage(MaterialEventMessage message)
        {
            return Task.CompletedTask;
        }

        private void ConfigureConsumer(IModel channel, ChannelConfig config, Func<MaterialEventMessage, Task> handler)
        {
            var channelConsumer = new AsyncEventingBasicConsumer(channel);

            channelConsumer.Received += async (sender, args) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(config.ExchangeName) && args.Exchange != config.ExchangeName)
                    {
                        throw new InvalidOperationException($"We received message from wrong exchange. Expected:{config.ExchangeName}. Received:{args.Exchange}");
                    }

                    if(args.Body.LongLength == 0)
                    {
                        throw new InvalidOperationException($"We received empty message.");
                    }


                    var message = BodyToObject<MaterialEventMessage>(args.Body, options); 
                    
                    await handler(message);

                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception {ex} during receiving message.", ex);

                    channel.BasicReject(args.DeliveryTag, false);
                }
            };
            
            channel.BasicConsume(queue: config.QueueName, false, consumer: channelConsumer);
        }
        private static void ConfigureExchange(IModel channel, ChannelConfig config)
        {
            if (channel is null || config is null) return;

            channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType ?? ExchangeType.Topic);
        }

        private static void ConfigureQueue(IModel channel, ChannelConfig config)
        {
            if (channel is null || config is null) return;

            config.QueueName = channel.QueueDeclare(
                config.QueueName ?? string.Empty,
                durable: durableQueue,
                exclusiveQueue,
                autoDeleteQueue).QueueName;

            if (string.IsNullOrWhiteSpace(config.ExchangeName) || !config.RoutingKeys.Any()) return;

            foreach (string routingKey in config.RoutingKeys)
            {
                channel.QueueBind(config.QueueName, config.ExchangeName, routingKey);
            }
        }

        private static T BodyToObject<T>(byte[] jsonBytes, JsonSerializerOptions deserializationOptions = null)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);

            return JsonSerializer.Deserialize<T>(json, deserializationOptions);
        }
    }
}