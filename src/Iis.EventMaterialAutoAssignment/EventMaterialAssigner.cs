using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
using Iis.RabbitMq.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Iis.EventMaterialAutoAssignment
{
    public class EventMaterialAssigner : BackgroundService
    {
        private const int RetryIntervalSec = 5;
        private readonly ILogger<EventMaterialAssigner> _logger;
        private readonly EventMaterialAssignerConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly INodeTypeLinked _eventType;
        private readonly INodeTypeLinked _eventComponentType;
        private readonly INodeTypeLinked _eventTypeType;
        private readonly INodeTypeLinked _eventImportanceType;
        private readonly INodeTypeLinked _countryType;
        private readonly INodeTypeLinked _eventStateType;
        private readonly INodeTypeLinked _accessLevelType;
        private readonly IConnection _connection;
        private readonly IModel _incommingChannel;

        public EventMaterialAssigner(
            ILogger<EventMaterialAssigner> logger,
            IConnectionFactory connectionFactory,
            EventMaterialAssignerConfiguration configuration,
            IOntologyNodesData ontologyData,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            _eventType = ontologyData.Schema.GetEntityTypeByName("Event");
            _eventComponentType = ontologyData.Schema.GetEntityTypeByName("EventComponent");
            _eventTypeType = ontologyData.Schema.GetEntityTypeByName("EventType");
            _eventImportanceType = ontologyData.Schema.GetEntityTypeByName("EventImportance");
            _countryType = ontologyData.Schema.GetEntityTypeByName("Country");
            _eventStateType = ontologyData.Schema.GetEntityTypeByName("EventState");
            _accessLevelType = ontologyData.Schema.GetEntityTypeByName("AccessLevel");

            _connection = connectionFactory.CreateAndWaitConnection(RetryIntervalSec, _logger, nameof(EventMaterialAssigner));

            _incommingChannel = _connection.CreateModel();
            _incommingChannel.QueueDeclare(
                queue: _configuration.IncomingQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channelConsumer = new AsyncEventingBasicConsumer(_incommingChannel);

            channelConsumer.Received += async (sender, args) =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AssignmentConfigContext>();
                        var ontologyService = scope.ServiceProvider.GetRequiredService<IOntologyService>();
                        var nodeMaterialRelationService = scope.ServiceProvider.GetRequiredService<NodeMaterialRelationService<IIISUnitOfWork>>();
                        var createEntityService = scope.ServiceProvider.GetRequiredService<CreateEntityService>();

                        var json = Encoding.UTF8.GetString(args.Body);
                        var message = JsonConvert.DeserializeObject<FoundMaterialMessage>(json);

                        var config = await GetAssignmentConfigById(context, message);
                        _logger.LogInformation("EventMaterialAssigner. Received messafe with confg id {configId}. Material ids {materialIds}", message.ConfigId, string.Join(',', message.MaterialIds));

                        if (config == null)
                        {
                            _logger.LogInformation("EventMaterialAssigner. Config with id {id} not found", message.ConfigId);
                            _incommingChannel.BasicReject(args.DeliveryTag, false);
                            return;
                        }

                        var foundEvent = ontologyService.GetNodeByUniqueValue(_eventType.Id, config.Name, "name");

                        if (foundEvent == null)
                        {
                            _logger.LogInformation("EventMaterialAssigner. Event with name {name} not found", config.Name);
                            var componentId = ontologyService.GetNodeByUniqueValue(_eventComponentType.Id, config.Component, "name")?.Id;
                            var typeId = ontologyService.GetNodeByUniqueValue(_eventTypeType.Id, config.EventType, "name")?.Id;
                            var importanceId = ontologyService.GetNodeByUniqueValue(_eventImportanceType.Id, config.Importance, "name")?.Id;
                            var countryId = ontologyService.GetNodeByUniqueValue(_countryType.Id, config.RelatesToCountry, "name")?.Id;
                            var stateId = ontologyService.GetNodeByUniqueValue(_eventStateType.Id, config.State, "name")?.Id;
                            var accessLevelId = ontologyService.GetNodeByUniqueValue(_accessLevelType.Id, config.AccessLevel, "name")?.Id;

                            if (componentId.HasValue && typeId.HasValue && importanceId.HasValue
                                && countryId.HasValue && stateId.HasValue && accessLevelId.HasValue)
                            {
                                _logger.LogInformation("EventMaterialAssigner. All necessary components to create event {name} are found", config.Name);
                                var properties = GenerateEventProperties(config, componentId, typeId, importanceId, countryId, stateId, accessLevelId);

                                var eventId = Guid.NewGuid();
                                await createEntityService.CreateEntity(eventId, _eventType, properties, null, eventId, Guid.NewGuid(), null);
                                _logger.LogInformation("EventMaterialAssigner. Event {name} created", config.Name);

                                await AssignEventToMaterial(nodeMaterialRelationService, message, eventId);
                            }
                        }
                        else
                        {
                            await AssignEventToMaterial(nodeMaterialRelationService, message, foundEvent.Id);
                        }

                        _incommingChannel.BasicAck(args.DeliveryTag, false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("EventMaterialAssigner. Exception {e}", e);
                    _incommingChannel.BasicReject(args.DeliveryTag, false);
                }
            };

            _incommingChannel.BasicConsume(_configuration.IncomingQueueName, false, channelConsumer);

            return Task.CompletedTask;
        }

        private static async Task<AssignmentConfig> GetAssignmentConfigById(AssignmentConfigContext context, FoundMaterialMessage message)
        {
            return await context.AssignmentConfigs
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.Id == message.ConfigId);
        }

        private static Dictionary<string, object> GenerateEventProperties(
            AssignmentConfig config,
            Guid? componentId,
            Guid? typeId,
            Guid? importanceId,
            Guid? countryId,
            Guid? stateId,
            Guid? accessLevelId)
        {
            var properties = new Dictionary<string, object>();
            properties.Add("name", config.Name);
            properties.Add("accessLevel", new Dictionary<string, object>() { { "targetId", accessLevelId.ToString() } });
            properties.Add("component", new Dictionary<string, object>() { { "targetId", componentId.ToString() } });
            properties.Add("eventType", new Dictionary<string, object>() { { "targetId", typeId.ToString() } });
            properties.Add("importance", new Dictionary<string, object>() { { "targetId", importanceId.ToString() } });
            properties.Add("relatesToCountry", new[] { new Dictionary<string, object>() { { "targetId", countryId.ToString() } } });
            properties.Add("state", new Dictionary<string, object>() { { "targetId", stateId.ToString() } });
            return properties;
        }

        private static async Task AssignEventToMaterial(NodeMaterialRelationService<IIISUnitOfWork> nodeMaterialRelationService, FoundMaterialMessage message, Guid eventId)
        {
            var nodesSet = new[] { eventId }.ToHashSet();
            var materialsSet = message.MaterialIds.ToHashSet();
            await nodeMaterialRelationService.CreateMultipleRelations(nodesSet, materialsSet, null);
        }
    }
}
