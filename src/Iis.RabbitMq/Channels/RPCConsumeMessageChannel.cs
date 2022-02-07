using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{
    public sealed class RPCConsumeMessageChannel<TRequest, TResponse> : IRPCConsumeMessageChannel<TRequest, TResponse>
    {
        private readonly string _consumerTag;
        private IConnection _connection;
        private RPCChannelConfig _config;
        private ILogger _logger;
        private IModel _channel;
        private Func<TRequest, Task<TResponse>> _onMessageReceived;

        public RPCConsumeMessageChannel(IConnection connection, RPCChannelConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _connection = connection;

            _channel = _connection.CreateModel();

            TopologyProvider.ConfugureConsuming(_channel, _config, QueryConfig.Default);

            _consumerTag = RegisterAsyncConsumerWithTag();
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.BasicCancel(_consumerTag);
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

        public void SetOnMessageReceived(Func<TRequest, Task<TResponse>> function) => _onMessageReceived = function;

        private string RegisterAsyncConsumerWithTag()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += AsyncEventHandler;

            return _channel.BasicConsume(_config.QueueName, false, consumer: consumer);
        }

        private async Task AsyncEventHandler(object sender, BasicDeliverEventArgs args)
        {
            var model = (sender as AsyncEventingBasicConsumer).Model;

            TResponse result = default(TResponse);

            try
            {
                if (args.Exchange != _config.ExchangeName)
                {
                    throw new InvalidOperationException($"Received message from wrong exchange. Expected:{_config.ExchangeName}. Received:{args.Exchange}");
                }

                var message = args.Body.ToObject<TRequest>(SerializationExtension.DefaultJsonSerializerOptions);

                if (message is null)
                {
                    throw new InvalidOperationException("No message is received.");
                }

                _logger?.LogInformation("Message {@message} has been received from exchange:{Exchange} with key:{RoutingKey}", message, args.Exchange, args.RoutingKey);

                var action = GetMessageAction();

                if (action != null)
                {
                    result = await action(message);
                }
            }
            catch (Exception exception)
            {
                result = default(TResponse);

                var messageState = args.ToState();

                _logger?.LogError("During processing message {@messageState} the exception has been thrown {@exception}", messageState, exception);
            }
            finally
            {
                var properties = args.BasicProperties;

                var replyProperties = model.CreateBasicProperties();

                replyProperties.CorrelationId = properties.CorrelationId;

                var body = result.ToByteArray(SerializationExtension.DefaultJsonSerializerOptions);

                model.BasicPublish(BaseChannelConfig.DefaultExchangeName, properties.ReplyTo, basicProperties: replyProperties, body);

                model.BasicAck(args.DeliveryTag, false);
            }
        }

        private Func<TRequest, Task<TResponse>> GetMessageAction()
        {
            return _onMessageReceived;
        }
    }
}