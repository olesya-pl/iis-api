using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using Iis.Api.Configuration;

namespace IIS.Core.Materials
{
    public interface IMaterialEventProducer : IDisposable
    {
        void SendMaterialAddedEventAsync(MaterialAddedEvent eventData);
        void SendMaterialEvent(MaterialEventMessage eventMessage);
        void SendMaterialFeatureEvent(MaterialEventMessage eventMessage);
        void SendAvailableForOperatorEvent(Guid materialId);
    }

    public class MaterialEventProducer : IMaterialEventProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;
        private readonly IModel _materialEventChannel;
        private readonly MaterialEventConfiguration _eventConfiguration;
        private readonly MaterialOperatorAssignerConfiguration _assignerConfiguration;

        public MaterialEventProducer(IConnectionFactory connectionFactory,
            ILoggerFactory loggerFactory,
            MaterialEventConfiguration eventConfiguration,
            MaterialOperatorAssignerConfiguration assignerConfiguration)
        {
            _logger = loggerFactory.CreateLogger<MaterialEventProducer>();

            _connectionFactory = connectionFactory;
            _eventConfiguration = eventConfiguration;
            _assignerConfiguration = assignerConfiguration;

            while (true)
            {
                try
                {
                    _connection = _connectionFactory.CreateConnection();
                    break;
                }
                catch (BrokerUnreachableException)
                {
                    var timeout = 5000;
                    _logger.LogError($"Attempting to connect again in {timeout / 1000} sec.");
                    Thread.Sleep(timeout);
                }
            }
            _channel = _connection.CreateModel();

            _materialEventChannel = ConfigChannel(_connection.CreateModel(), _eventConfiguration.TargetChannel);
        }

        public void SendMaterialAddedEventAsync(MaterialAddedEvent eventData)
        {
            _channel.QueueDeclare("gsm", true, false);

            var json = JObject.FromObject(eventData).ToString();
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                routingKey: "gsm",
                                basicProperties: null,
                                body: body);
        }

        public void SendMaterialEvent(MaterialEventMessage eventMessage)
        {
            var routingKey = $"processing.ml.{eventMessage.Type}";

            SendMaterialEventMessage(eventMessage, routingKey);
        }

        public void SendMaterialFeatureEvent(MaterialEventMessage eventMessage)
        {
            var routingKey = $"processing.features.{eventMessage.Type}";

            SendMaterialEventMessage(eventMessage, routingKey);
        }

        private void SendMaterialEventMessage(MaterialEventMessage message, string routingKey)
        {
            var json = JObject.FromObject(message).ToString();

            var body = Encoding.UTF8.GetBytes(json);

            var properties = _materialEventChannel.CreateBasicProperties();

            properties.Persistent = true;

            _materialEventChannel.BasicPublish(exchange: _eventConfiguration.TargetChannel.ExchangeName,
                                routingKey: routingKey,
                                basicProperties: null,
                                body: body);
        }
        private IModel ConfigChannel(IModel channel, ChannelConfig config)
        {
            if(config is null) return channel;

            channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType ?? ExchangeType.Topic);

            return channel;
        }

        public void Dispose()
        {
            _channel.Dispose();
            _materialEventChannel.Dispose();
            _connection.Dispose();
        }
        public void SendAvailableForOperatorEvent(Guid materialId)
        {
            _channel.QueueDeclare(
                queue: _assignerConfiguration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(materialId.ToString());

            _channel.BasicPublish(exchange: "",
                                routingKey: _assignerConfiguration.QueueName,
                                basicProperties: null,
                                body: body);
        }
    }
}
