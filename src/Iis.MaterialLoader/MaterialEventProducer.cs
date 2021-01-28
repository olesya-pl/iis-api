using System;
using System.Threading;
using Iis.Messages;
using Iis.Utility;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Iis.MaterialLoader
{
    public interface IMaterialEventProducer : IDisposable
    {
        void PublishMaterialCreatedMessage(MaterialCreatedMessage message);
    }

    public class MaterialEventProducer : IMaterialEventProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;

        public MaterialEventProducer(IConnectionFactory connectionFactory,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MaterialEventProducer>();

            _connectionFactory = connectionFactory;

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

        }
        
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
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
    }
}
