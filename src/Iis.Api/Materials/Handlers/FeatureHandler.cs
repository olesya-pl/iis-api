using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Materials.FeatureProcessors;
using IIS.Core.Materials.Handlers.Configurations;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Constants;
using Iis.RabbitMq.Channels;
using Iis.RabbitMq.Helpers;
using IIS.Repository.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Iis.Messages.Materials;

namespace Iis.Api.Materials.Handlers
{
    public class FeatureHandler : BackgroundService
    {
        private const bool exclusiveQueue = false;
        private const bool durableQueue = true;
        private const bool autoDeleteQueue = false;
        private const int ReConnectTimeoutSec = 5;
        private readonly ILogger<FeatureHandler> _logger;
        private readonly FeatureHandlerConfig _сonfig;
        private readonly IServiceProvider _provider;
        private readonly IUnitOfWorkFactory<IIISUnitOfWork> _unitOfWorkFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly JsonSerializerOptions options;
        public FeatureHandler(ILogger<FeatureHandler> logger,
            IConnectionFactory connectionFactory,
            FeatureHandlerConfig configuration,
            IServiceProvider provider,
            IUnitOfWorkFactory<IIISUnitOfWork> unitOfWorkFactory)
        {
            _logger = logger;
            _сonfig = configuration;
            _provider = provider;
            _unitOfWorkFactory = unitOfWorkFactory;

            _connection = connectionFactory.CreateAndWaitConnection(ReConnectTimeoutSec, logger);

            _channel = _connection.CreateModel();

            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConfigureExchange(_channel, _сonfig.SourceChannel);

            ConfigureQueue(_channel, _сonfig.SourceChannel);

            ConfigureConsumer(_channel, _сonfig.SourceChannel, ProcessMessage);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _channel.Dispose();

            _connection.Close();
            _connection.Dispose();

            _channel = null;
            _connection = null;

            base.Dispose();
        }

        private async Task ProcessMessage(MaterialProcessingEventMessage message)
        {
            var processor = _provider.GetService<IFeatureProcessorFactory>().GetInstance(message.Source, message.Type);

            if(processor.IsDummy) return;

            var material = await RunAsync(uow => uow.MaterialRepository.GetByIdAsync(message.Id));

            if(material is null) return;

            JObject metadata = JObject.Parse(material.Metadata);

            metadata = await processor.ProcessMetadataAsync(metadata, material.Id);

            material.Metadata = metadata.ToString(Newtonsoft.Json.Formatting.None);

            var featureIdList = GetNodeIdentitiesFromFeatures(metadata);

            RunWithCommit(uow => {
                uow.MaterialRepository.AddFeatureIdList(material.Id, featureIdList);
                uow.MaterialRepository.EditMaterial(material);
            });
        }

        private IEnumerable<Guid> GetNodeIdentitiesFromFeatures(JObject metadata)
        {
            var result = new List<Guid>();

            var features = metadata.SelectToken(FeatureFields.FeaturesSection);

            if (features is null) return result;

            foreach (JObject feature in features)
            {
                var featureId = feature.GetValue(FeatureFields.featureId)?.Value<string>();

                if (string.IsNullOrWhiteSpace(featureId)) continue;

                if (!Guid.TryParse(featureId, out Guid featureGuid)) continue;

                if (featureGuid.Equals(Guid.Empty)) continue;

                result.Add(featureGuid);
            }

            return result.Distinct();
        }

        private async Task<T> RunAsync<T>(Func<IIISUnitOfWork, Task<T>> action)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                return await action(unitOfWork);
            }
        }

        private async Task RunWithCommitAsync(Func<IIISUnitOfWork, Task> action)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await action(unitOfWork);
                await unitOfWork.CommitAsync();
            }
        }

        private void RunWithCommit(Action<IIISUnitOfWork> action)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                action(unitOfWork);
                unitOfWork.Commit();
            }
        }

        private void ConfigureConsumer(IModel channel, ChannelConfig config, Func<MaterialProcessingEventMessage, Task> handler)
        {
            var channelConsumer = new AsyncEventingBasicConsumer(channel);

            channelConsumer.Received += async (sender, args) =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(config.ExchangeName) && args.Exchange != config.ExchangeName)
                    {
                        throw new InvalidOperationException($"We received message from wrong exchange. Expected:{config.ExchangeName}. Received:{args.Exchange}");
                    }

                    if(args.Body.LongLength == 0)
                    {
                        throw new InvalidOperationException($"We received empty message.");
                    }


                    var message = BodyToObject<MaterialProcessingEventMessage>(args.Body, options);

                    await handler(message);

                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception {ex} during receiving message.", ex);

                    channel.BasicReject(args.DeliveryTag, false);
                }
            };

            channel.BasicConsume(queue: config.QueueName, false, consumer: channelConsumer);
        }

        private static void ConfigureExchange(IModel channel, ChannelConfig config)
        {
            if (channel is null || config is null) return;

            channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType ?? ExchangeType.Topic);
        }

        private static void ConfigureQueue(IModel channel, ChannelConfig config)
        {
            if (channel is null || config is null) return;

            config.QueueName = channel.QueueDeclare(
                config.QueueName ?? string.Empty,
                durable: durableQueue,
                exclusiveQueue,
                autoDeleteQueue).QueueName;

            channel.BasicQos(0, config.PrefetchCount, global:false);

            if (string.IsNullOrWhiteSpace(config.ExchangeName) || !config.RoutingKeys.Any()) return;

            foreach (string routingKey in config.RoutingKeys)
            {
                channel.QueueBind(config.QueueName, config.ExchangeName, routingKey);
            }
        }

        private static T BodyToObject<T>(byte[] jsonBytes, JsonSerializerOptions deserializationOptions = null)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);

            return JsonSerializer.Deserialize<T>(json, deserializationOptions);
        }
    }
}