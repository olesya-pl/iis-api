using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        void SendMaterialEvent(MaterialProcessingEventMessage eventMessage);
        void SendMaterialProcessingEvent(MaterialProcessingEventMessage eventMessage);
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
        private readonly IPublishMessageChannel<MaterialProcessingEventMessage> _eventPublishChannel;
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

            _eventPublishChannel = new PublishMessageChannel<MaterialProcessingEventMessage>(_connection, _eventConfiguration.TargetChannel);
        }

        public void SendMaterialEvent(MaterialProcessingEventMessage eventMessage)
        {
            var mlRoutingKey = $"processing.ml.{eventMessage.Type}";

            _eventPublishChannel.Send(eventMessage, mlRoutingKey);

            _logger.LogInformation($"sending material with id {eventMessage.Id} for processing: ML key:[{mlRoutingKey}]");
        }

        public void SendMaterialProcessingEvent(MaterialProcessingEventMessage eventMessage)
        {

            var mlRoutingKey = $"processing.ml.{eventMessage.Type}";

            var featuresRoutingKey = $"processing.features.{eventMessage.Type}";

            var coordinatesRoutingKey = "processing.coordinates";

            _eventPublishChannel.Send(eventMessage, mlRoutingKey, featuresRoutingKey, coordinatesRoutingKey);

            _logger.LogInformation($"sending material with id {eventMessage.Id} for processing: ML key:[{mlRoutingKey}], Features key:[{featuresRoutingKey}], Coordinates key:[{coordinatesRoutingKey}]");
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
