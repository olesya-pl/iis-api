using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
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
        private readonly IMaterialElasticService _materialElasticService;
        private readonly MaterialElasticSaverConfiguration _configuration;
        private readonly IMaterialEventProducer _materialEventProducer;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly List<Guid> _materialIds = new List<Guid>();

        public MaterialElasticConsumer(ILogger<MaterialElasticConsumer> logger,
            IConnectionFactory connectionFactory,
            IMaterialElasticService materialElasticService,
            IMaterialEventProducer materialEventProducer,
            MaterialElasticSaverConfiguration configuration)
        {
            _logger = logger;
            _materialElasticService = materialElasticService;
            _configuration = configuration;
            _materialEventProducer = materialEventProducer;

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
                            await _materialElasticService.PutCreatedMaterialsToElasticSearchAsync(_materialIds, true, stoppingToken);
                            _materialEventProducer.SendMaterialSavedToElastic(_materialIds);
                            _materialIds.Clear();
                        }                        
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);                        
                        continue;
                    }
                    var materialId = new Guid(Encoding.UTF8.GetString(message.Body));
                    _channel.BasicAck(message.DeliveryTag, false);
                    _materialIds.Add(materialId);
                    if (_materialIds.Count() >= MaxBatchSize)
                    {
                        await _materialElasticService.PutCreatedMaterialsToElasticSearchAsync(_materialIds, true, stoppingToken);
                        _materialEventProducer.SendMaterialSavedToElastic(_materialIds);
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