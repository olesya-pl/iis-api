using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace IIS.Replication
{
    public class ReplicationHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IReplicationService _replicationService;

        public ReplicationHostedService(ILoggerFactory loggerFactory, ConnectionFactory connectionFactory,
            IReplicationService replicationService)
        {
            _logger = loggerFactory.CreateLogger<ReplicationHostedService>();
            _connectionFactory = connectionFactory;
            _replicationService = replicationService;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            // create connection  
            while (true)
            {
                try
                {
                    _connection = _connectionFactory.CreateConnection();
                    break;
                }
                catch (BrokerUnreachableException ex)
                {
                    var timeout = 5000;
                    _logger.LogError($"Attempting to connect again in {timeout / 1000} sec.");
                    Thread.Sleep(timeout);
                }
            }

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare(queue: "entities",
                 durable: false,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);

            //_channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            //_channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var message = System.Text.Encoding.UTF8.GetString(ea.Body);
                _logger.LogInformation(message);
                _replicationService.IndexEntity(message);
                _channel.BasicAck(ea.DeliveryTag, false);

                _logger.LogInformation("*********SAVED********");
            };

            //consumer.Shutdown += OnConsumerShutdown;
            //consumer.Registered += OnConsumerRegistered;
            //consumer.Unregistered += OnConsumerUnregistered;
            //consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(queue: "entities",
                                 autoAck: false,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
