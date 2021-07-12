using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Iis.Messages;
using Iis.Utility;
using Iis.RabbitMq.Channels;
using Iis.RabbitMq.Helpers;
using Iis.Messages.Materials;
using Iis.Services.Contracts.Configurations;

namespace IIS.Core.Materials
{
    public interface IMaterialEventProducer : IDisposable
    {
        void SendMaterialEvent(MaterialEventMessage eventMessage);
        void SendMaterialFeatureEvent(MaterialEventMessage eventMessage);
        void SendMaterialCoordinateEvent(MaterialEventMessage eventMessage);
        void SendAvailableForOperatorEvent(Guid materialId);
        void SaveMaterialToElastic(Guid id);
        void SendMaterialSavedToElastic(List<Guid> ids);
        void PublishMaterialCreatedMessage(MaterialCreatedMessage message);
    }

    public class MaterialEventProducer : IMaterialEventProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;
        private readonly IModel _materialEventChannel;
        private readonly IPublishMessageChannel<MaterialEventMessage> _eventPublishChannel;
        private readonly MaterialEventConfiguration _eventConfiguration;
        private readonly MaterialOperatorAssignerConfiguration _assignerConfiguration;
        private readonly MaterialElasticSaverConfiguration _elasticSaverConfiguration;

        public MaterialEventProducer(IConnectionFactory connectionFactory,
            ILoggerFactory loggerFactory,
            MaterialEventConfiguration eventConfiguration,
            MaterialOperatorAssignerConfiguration assignerConfiguration,
            MaterialElasticSaverConfiguration elasticSaverConfiguration)
        {
            _logger = loggerFactory.CreateLogger<MaterialEventProducer>();

            _connectionFactory = connectionFactory;
            _eventConfiguration = eventConfiguration;
            _assignerConfiguration = assignerConfiguration;
            _elasticSaverConfiguration = elasticSaverConfiguration;

            _connection = _connectionFactory.CreateAndWaitConnection();

            _channel = _connection.CreateModel();

            _materialEventChannel = ConfigChannel(_connection.CreateModel(), _eventConfiguration.TargetChannel);

            _eventPublishChannel = new PublishMessageChannel<MaterialEventMessage>(_connection, _eventConfiguration.TargetChannel);
        }

        public void SendMaterialEvent(MaterialEventMessage eventMessage)
        {
            _logger.LogInformation($"sending material with id {eventMessage.Id} for ML processing");

            var routingKey = $"processing.ml.{eventMessage.Type}";

            SendMaterialEventMessage(eventMessage, routingKey);
        }

        public void SendMaterialFeatureEvent(MaterialEventMessage eventMessage)
        {
            _logger.LogInformation($"sending material with id {eventMessage.Id} for feature processing");

            var routingKey = $"processing.features.{eventMessage.Type}";

            SendMaterialEventMessage(eventMessage, routingKey);
        }

        public void SendMaterialCoordinateEvent(MaterialEventMessage eventMessage)
        {
            _logger.LogInformation($"sending material with id {eventMessage.Id} for fetching coordinates");

            _eventPublishChannel.Send(eventMessage, "processing.coordinates");
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
            _eventPublishChannel.Dispose();
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

        public void SaveMaterialToElastic(Guid materialId)
        {
            _channel.QueueDeclare(
                queue: _elasticSaverConfiguration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(materialId.ToString());

            _channel.BasicPublish(exchange: "",
                                routingKey: _elasticSaverConfiguration.QueueName,
                                basicProperties: null,
                                body: body);
        }

        public void PublishMaterialCreatedMessage(MaterialCreatedMessage message)
        {
            _channel.QueueDeclare(
                queue: MaterialRabbitConsts.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = message.ToBytes();

            _channel.BasicPublish(exchange: "",
                routingKey: MaterialRabbitConsts.QueueName,
                basicProperties: null,
                body: body);
        }

        public void SendMaterialSavedToElastic(List<Guid> ids)
        {
            _channel.QueueDeclare(queue: _elasticSaverConfiguration.OutgoingQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(_elasticSaverConfiguration.OutgoingQueueName, _elasticSaverConfiguration.OutgoingExchangeName, _elasticSaverConfiguration.OutgoingRoutingKey);

            var json = JsonConvert.SerializeObject(ids);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: _elasticSaverConfiguration.OutgoingExchangeName,
                                routingKey: _elasticSaverConfiguration.OutgoingRoutingKey,
                                basicProperties: null,
                                body: body);
        }
    }
}
