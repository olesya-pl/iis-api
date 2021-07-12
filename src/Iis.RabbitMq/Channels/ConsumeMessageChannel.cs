using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{

    public class ConsumeMessageChannel<T> : IConsumeMessageChannel<T>
    {
        private const bool exclusiveQueue = false;
        private const bool durableQueue = true;
        private const bool autoDeleteQueue = false;
        private ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private ChannelConfig _config;
        private string _consumerTag;
        private JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private Func<T, Task> _onMessageReceived;

        public Func<T, Task> OnMessageReceived
        { 
            set
            {
                if(value is null) return;

                _onMessageReceived = value;
            }
        }

        public ConsumeMessageChannel(IConnection connection, ChannelConfig config, ILogger logger)
        {
            _logger = logger;
            _connection = connection;
            _config = config;
            _channel = ConfigureTopology(_connection.CreateModel(), config);

            ConfigureConsumer(_channel, _config, _options, _logger);
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }

            if(_onMessageReceived != null)
            {
                _onMessageReceived = null;
            }

            _connection = null;
        }

        private IModel ConfigureTopology(IModel channel, ChannelConfig config)
        {
            if (config is null || channel is null) return channel;

            if (config.IsNotDefaultExchangeUsed())
            {
                channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType ?? ExchangeType.Topic);
            }

            channel.BasicQos(0, config.PrefetchCount, false);

            config.QueueName = channel.QueueDeclare(config.QueueName ?? string.Empty,
                                                    durable: durableQueue,
                                                    exclusive: exclusiveQueue,
                                                    autoDelete: autoDeleteQueue).QueueName;

            foreach (var routingKey in config.RoutingKeys)
            {
                channel.QueueBind(config.QueueName, config.ExchangeName, routingKey);
            }

            return channel;
        }

        private void ConfigureConsumer(IModel channel, ChannelConfig config, JsonSerializerOptions options, ILogger logger)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            var handler = GetConsumerReceivedHandler(config, options, logger);

            consumer.Received += handler;

            _consumerTag = channel.BasicConsume(queue: config.QueueName, false, consumer: consumer);
        }

        private AsyncEventHandler<BasicDeliverEventArgs> GetConsumerReceivedHandler(ChannelConfig config, JsonSerializerOptions options, ILogger logger)
        {
            return async (sender, args) =>
            {
                if (args.Exchange != config.ExchangeName)
                {
                    throw new InvalidOperationException($"We received message from wrong exchange. Expected:{config.ExchangeName}. Received:{args.Exchange}");
                }

                var message = args.Body.ToObject<T>(options);

                if (message is null)
                { 
                    throw new InvalidOperationException("No message is received.");
                }

                logger.LogInformation("Message {@message} has been received from exchange:{Exchange} with key:{RoutingKey}", message, args.Exchange, args.RoutingKey);

                var model = (sender as AsyncEventingBasicConsumer).Model;

                var userAction = GetUserActon();

                try
                {
                    if (userAction != null)
                    {
                        await userAction(message);
                    }

                    model.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception during processing message. {ex}", ex);
                    model.BasicReject(args.DeliveryTag, false);
                }
            };
        }

        private Func<T, Task> GetUserActon()
        {
            return _onMessageReceived;
        }
    }
}