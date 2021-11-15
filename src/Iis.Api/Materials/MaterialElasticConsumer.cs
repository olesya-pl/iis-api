using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Iis.RabbitMq.Helpers;
using Iis.Messages.Materials;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.Materials;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Iis.Api.Materials
{
    public class MaterialElasticConsumer : BackgroundService
    {
        private const int RetryIntervalSec = 5;
        private const int MaxBatchSize = 50;
        private readonly ILogger<MaterialElasticConsumer> _logger;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly MaterialElasticSaverConfiguration _configuration;
        private readonly IMaterialEventProducer _materialEventProducer;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly List<MaterialProcessingEventMessage> _materialMessageList = new List<MaterialProcessingEventMessage>(MaxBatchSize);
        private readonly List<Guid> _materialIdList = new List<Guid>(MaxBatchSize);
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private static readonly TimeSpan LowIntenseDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan HighIntenseDelay = TimeSpan.FromMilliseconds(100);

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

            _connection = connectionFactory.CreateAndWaitConnection(RetryIntervalSec, _logger, nameof(MaterialElasticConsumer));

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: _configuration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                _configuration.QueueName,
                _configuration.OutgoingExchangeName,
                _configuration.QueueName);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            BasicGetResult getResult = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    getResult = _channel.BasicGet(_configuration.QueueName, false);

                    if (getResult != null)
                    {
                        var message = GetEventMessage(getResult.Body);

                        _channel.BasicAck(getResult.DeliveryTag, false);

                        _materialMessageList.Add(message);
                        _materialIdList.Add(message.Id);
                    }

                    var beginProcessingResult = CalculateBeginProcessingResult(getResult, _materialMessageList.Count);

                    if(beginProcessingResult.BeginProcessing)
                    {
                            await _materialElasticService.PutCreatedMaterialsToElasticSearchAsync(_materialIdList, true, stoppingToken);
                            _materialEventProducer.SendMaterialSavedToElastic(_materialIdList);
                            _materialEventProducer.SendMaterialProcessingEvents(_materialMessageList);
                            _materialMessageList.Clear();
                            _materialIdList.Clear();
                    }

                    await Task.Delay(beginProcessingResult.nextStepDelay, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError("MaterialElasticSaver. Exception={e}", e);

                    if(getResult != null && _channel.IsOpen)
                    {
                        _channel.BasicReject(getResult.DeliveryTag, true);
                    }
                }
            }
        }

        private MaterialProcessingEventMessage GetEventMessage(byte[] messageBody)
        {
            if (messageBody is null || messageBody.Length == 0) return default(MaterialProcessingEventMessage);

            var messageBodyString = Encoding.UTF8.GetString(messageBody);

            return JsonSerializer.Deserialize<MaterialProcessingEventMessage>(messageBodyString, _options);
        }

        private (bool BeginProcessing, TimeSpan nextStepDelay) CalculateBeginProcessingResult(BasicGetResult message, int eventMessageCount)
        {
            if (message is null && eventMessageCount > 0) return (true, LowIntenseDelay);

            if (message != null && eventMessageCount >= MaxBatchSize) return (true, HighIntenseDelay);

            return (false, LowIntenseDelay);
        }
    }
}