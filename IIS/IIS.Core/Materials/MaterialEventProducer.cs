using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IIS.Core.GraphQL.Materials;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace IIS.Core.GSM.Producer
{
    public class MaterialAddedEvent
    {
        public Guid FileId;
        public Guid MaterialId;
        public List<IIS.Core.GraphQL.Materials.Node> Nodes { get; set; }
    }

    public interface IMaterialEventProducer : IDisposable
    {
        void SendMaterialAddedEventAsync(MaterialAddedEvent eventData);
    }

    public class MaterialEventProducer : IMaterialEventProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;

        public MaterialEventProducer(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
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

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
