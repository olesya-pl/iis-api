using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Materials;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Iis.Api.Materials
{
    public class MaterialElasticConsumer : BackgroundService
    {
        private readonly ILogger<MaterialElasticConsumer> _logger;
        private readonly IMaterialService _materialService;
        private readonly CreatedMaterialElasticSaverConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly List<Guid> _materialIds = new List<Guid>();

        public MaterialElasticConsumer(ILogger<MaterialElasticConsumer> logger,
            IConnectionFactory connectionFactory,
            IMaterialService materialService,
            CreatedMaterialElasticSaverConfiguration configuration)
        {
            _logger = logger;
            _materialService = materialService;
            _configuration = configuration;

            while (true)
            {
                try
                {
                    _connection = connectionFactory.CreateConnection();
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
            _channel.QueueDeclare(
                queue: _configuration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int MaxBatchSize = 50;
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = _channel.BasicGet(_configuration.QueueName, false);
                    if (message == null)
                    {
                        if (_materialIds.Any())
                        {
                            await _materialService.PutCreatedMaterialsToElasticSearchAsync(_materialIds, stoppingToken);
                            _materialIds.Clear();
                        }                        
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);                        
                        continue;
                    }
                    var materialId = new Guid(System.Text.Encoding.UTF8.GetString(message.Body));
                    _channel.BasicAck(message.DeliveryTag, false);
                    _materialIds.Add(materialId);
                    if (_materialIds.Count() >= MaxBatchSize)
                    {
                        await _materialService.PutCreatedMaterialsToElasticSearchAsync(_materialIds, stoppingToken);
                        _materialIds.Clear();
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError("MaterialElasticSaver. Exception={e}", e);
                }
            }
        }
    }
}
