using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{
    public sealed class ConsumeMessageChannel<T> : IConsumeMessageChannel<T>
    {
        private ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private ChannelConfig _config;
        private string _consumerTag;
        private JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private Func<T, Task> _onMessageReceived;

        public ConsumeMessageChannel(IConnection connection, ChannelConfig config, ILogger logger)
        {
            _logger = logger;
            _connection = connection;
            _config = config;
            _channel = _connection.CreateModel();

            TopologyProvider.ConfugureConsuming(_channel, _config, QueryConfig.DefaultDurable);

            ConfigureConsumer(_channel, _config, _options, _logger);
        }

        public Func<T, Task> OnMessageReceived
        {
            set
            {
                if (value is null) return;

                _onMessageReceived = value;
            }
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }

            if (_onMessageReceived != null)
            {
                _onMessageReceived = null;
            }

            _connection = null;
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
                var model = (sender as AsyncEventingBasicConsumer).Model;

                try
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

                    logger?.LogInformation("Message {@message} has been received from exchange:{Exchange} with key:{RoutingKey}", message, args.Exchange, args.RoutingKey);

                    var userAction = GetUserActon();

                    if (userAction != null)
                    {
                        await userAction(message);
                    }

                    model.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    var messageState = args.ToState();

                    logger?.LogError("During processing message {@messageState} the exception has been thrown {@ex}", messageState, ex);

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