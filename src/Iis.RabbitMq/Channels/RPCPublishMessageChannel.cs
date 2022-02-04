using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{
    public sealed class RPCPublishMessageChannel<TRequest, TResponse> : IRPCPublishMessageChannel<TRequest, TResponse>
    {
        private readonly string _consumerTag;
        private readonly string _replyQueueName;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<TResponse>> _callBackDictionary =
            new ConcurrentDictionary<string, TaskCompletionSource<TResponse>>();
        private IConnection _connection;
        private RPCChannelConfig _сonfig;
        private ILogger _logger;
        private IModel _channel;

        public RPCPublishMessageChannel(IConnection connection, RPCChannelConfig сonfig, ILogger logger)
        {
            _connection = connection;
            _сonfig = сonfig;
            _logger = logger;
            _channel = _connection.CreateModel();

            TopologyProvider.ConfigurePublishing(_channel, _сonfig);

            _replyQueueName = _channel.QueueDeclare().QueueName;

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

            _connection = null;
        }

        public Task<TResponse> SendAsync(TRequest message, CancellationToken cancellationToken = default)
        {
            return SendAsync(message, _сonfig.RoutingKey, cancellationToken);
        }

        public Task<TResponse> SendAsync(TRequest message, string routingKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentException("Routing key is null or empty.", nameof(routingKey));
            }

            if (message is null)
            {
                throw new ArgumentException("No message is defined.", nameof(message));
            }

            var body = message.ToByteArray(SerializationExtension.DefaultJsonSerializerOptions);

            var properties = GetBasicProperties();

            var taskCompletionSource = new TaskCompletionSource<TResponse>();

            _callBackDictionary.TryAdd(properties.CorrelationId, taskCompletionSource);

            _channel.BasicPublish(_сonfig.ExchangeName, routingKey, basicProperties: properties, body);

            cancellationToken.Register(() => _callBackDictionary.TryRemove(properties.CorrelationId, out var _));

            return taskCompletionSource.Task;
        }

        private string RegisterAsyncConsumerWithTag()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += AsyncRPCConsumerHandler;

            return _channel.BasicConsume(_replyQueueName, true, consumer: consumer);
        }

        private IBasicProperties GetBasicProperties()
        {
            var properties = _channel.CreateBasicProperties();

            properties.CorrelationId = Guid.NewGuid().ToString();
            properties.ReplyTo = _replyQueueName;

            return properties;
        }

        private Task AsyncRPCConsumerHandler(object sender, BasicDeliverEventArgs args)
        {
            var model = (sender as AsyncEventingBasicConsumer).Model;

            if (!_callBackDictionary.TryRemove(args.BasicProperties.CorrelationId, out TaskCompletionSource<TResponse> tcs))
            {
                return Task.CompletedTask;
            }

            try
            {
                var message = args.Body.ToObject<TResponse>(SerializationExtension.DefaultJsonSerializerOptions);

                if (message is null)
                {
                    throw new InvalidOperationException($"No message is received or type {nameof(TResponse)}");
                }

                _logger?.LogInformation("Message {@message} has been received from exchange:{Exchange} with key:{RoutingKey}", message, args.Exchange, args.RoutingKey);

                tcs.TrySetResult(message);
            }
            catch (Exception exception)
            {
                var messageState = args.ToState();

                _logger?.LogError("During processing message {@messageState} the exception has been thrown {@ex}", messageState, exception);

                tcs.SetException(exception);
            }

            return Task.CompletedTask;
        }
    }
}