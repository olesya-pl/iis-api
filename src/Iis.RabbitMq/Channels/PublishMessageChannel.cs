using System;
using System.Linq;
using RabbitMQ.Client;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{
    public sealed class PublishMessageChannel<T> : IPublishMessageChannel<T>
    {
        private IConnection _connection;
        private ChannelConfig _config;
        private IModel _channel;

        public PublishMessageChannel(IConnection connection, ChannelConfig config)
        {
            _connection = connection;
            _config = config;
            _channel = _connection.CreateModel();

            TopologyProvider.ConfigurePublishing(_channel, _config);
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }

            _connection = null;
        }

        public void Send(T message)
        {
            Send(message, _config.RoutingKeys.ToArray());
        }

        public void Send(T message, params string[] routingKeyList)
        {
            if (routingKeyList is null || !routingKeyList.Any())
            {
                throw new ArgumentException("List of routing keys is null or empty.", nameof(routingKeyList));
            }

            if (message is null)
            {
                throw new ArgumentException("No message is defined.", nameof(message));
            }

            var body = message.ToByteArray(SerializationExtension.DefaultJsonSerializerOptions);

            var model = GetModel();

            var properties = model.CreateBasicProperties();

            properties.Persistent = true;

            foreach (string routingKey in routingKeyList)
            {
                model.BasicPublish(_config.ExchangeName, routingKey, basicProperties: properties, body);
            }
        }

        private IModel GetModel()
        {
            return _channel switch
            {
                null => _channel = _connection.CreateModel(),
                var channel when !channel.IsOpen => _channel = _connection.CreateModel(),
                _ => _channel
            };
        }
    }
}