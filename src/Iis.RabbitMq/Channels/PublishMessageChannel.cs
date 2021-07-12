using System;
using System.Linq;
using System.Text.Json;
using RabbitMQ.Client;
using Iis.RabbitMq.Helpers;

namespace Iis.RabbitMq.Channels
{

    public class PublishMessageChannel<T> : IPublishMessageChannel<T>
    {
        private IConnection _connection;
        private ChannelConfig _config;
        private IModel _channel;
        private readonly JsonSerializerOptions _options;

        public PublishMessageChannel(IConnection connection, ChannelConfig config)
        {
            _connection = connection;
            _config = config;

            _channel = ConfigureTopology(connection.CreateModel(), config);

            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public void Dispose()
        {
            if(_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }

            _connection = null;
        }

        public void Send(T message)
        {
            Send(message, _config.RoutingKeys);
        }

        public void Send(T message, params string[] routingKeyList)
        {
            if(routingKeyList is null || !routingKeyList.Any()) throw new ArgumentException("List of routing keys is null or empty.", nameof(routingKeyList));

            if(message is null) throw new ArgumentException("No message is defined.", nameof(message));

            var body = message.ToMessage(_options);

            foreach(string routingKey in routingKeyList)
            {
                GetModel().BasicPublish(_config.ExchangeName, routingKey, basicProperties: null, body);
            }
        }

        private IModel ConfigureTopology(IModel model, ChannelConfig config)
        {
            if(config is null) return model;

            model.ExchangeDeclare(config.ExchangeName, config.ExchangeType ?? ExchangeType.Topic);

            return model;
        }

        private IModel GetModel()
        {
            return _channel switch
            {
                null => (_channel = _connection.CreateModel()),
                var ch when !ch.IsOpen => (_channel = _connection.CreateModel()),
                _ => _channel
            };
        }
    }
}