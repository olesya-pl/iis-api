using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Domain;
using Iis.Domain.Users;
using Iis.Events.Entities;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using Attribute = Iis.Domain.Attribute;

namespace Iis.Services
{
    public class CreateEntityService
    {
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyService _ontologyService;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IFileService _fileService;
        private readonly IMediator _mediator;
        private readonly IMaterialElasticService _materialElasticService;

        public CreateEntityService(IOntologySchema ontologySchema,
            IOntologyService ontologyService,
            IChangeHistoryService changeHistoryService,
            IFileService fileService,
            IMediator mediator,
            IMaterialElasticService materialElasticService)
        {
            _ontologySchema = ontologySchema;
            _ontologyService = ontologyService;
            _changeHistoryService = changeHistoryService;
            _fileService = fileService;
            _mediator = mediator;
            _materialElasticService = materialElasticService;
        }

        public async Task<Entity> CreateEntity(Guid rootEntityId,
                INodeTypeLinked type,
                Dictionary<string, object> properties,
                string dotName,
                Guid entityId,
                Guid requestId,
                User currentUser)
        {
            if (properties == null)
                throw new ArgumentException($"Cannot create {type.Name} because no properties provided.");
            Entity node;
            if (type.HasUniqueValues)
            {
                node = GetUniqueValueEntity(type, properties);
                if (node != null) return node;
            }

            node = new Entity(entityId, type);
            await CreateProperties(rootEntityId, node, properties, dotName, requestId, currentUser);
            SetCreatedByIfExists(node, currentUser);
            SetLastConfirmedAtIfExists(node);
            _ontologyService.SaveNode(node);
            await _mediator.Publish(new EntityCreatedEvent() { Type = type.Name, Id = node.Id });
            await PutLinkedMaterialsToElasticAsync(node.Id);
            return node;
        }

        public Task<Relation[]> CreateMultipleProperties(
            Guid entityId,
            INodeTypeLinked embed,
            object value,
            string oldValue,
            string dotName,
            Guid requestId,
            User currentUser)
        {
            var values = (IEnumerable<object>)value;
            var result = new List<Task<Relation>>();
            foreach (var v in values)
                if (embed.IsEntityType)
                {
                    result.Add(CreateSinglePropertyAsync(entityId, embed, v, oldValue, dotName, requestId, currentUser));
                }
                else
                {
                    var dict = (Dictionary<string, object>)v;
                    var attrValue = dict["value"];
                    result.Add(CreateSinglePropertyAsync(entityId, embed, attrValue, oldValue, dotName, requestId, currentUser));
                }

            return Task.WhenAll(result);
        }

        public async Task<Relation> CreateSinglePropertyAsync(
            Guid entityId,
            INodeTypeLinked embed,
            object value,
            object oldValue,
            string dotName,
            Guid requestId,
            User currentUser)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(entityId, embed, value, oldValue, dotName, requestId, currentUser);
            prop.AddNode(target);
            return prop;
        }

        private async Task<Entity> CreateProperties(Guid entityId,
            Entity node,
            Dictionary<string, object> properties,
            string dotName,
            Guid requestId,
            User currentUser)
        {
            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                var newDotName = string.IsNullOrEmpty(dotName)
                    ? key
                    : $"{dotName}.{key}";
                var relations = await CreateRelations(entityId, embed, value, newDotName, requestId, currentUser);
                foreach (var relation in relations)
                    node.AddNode(relation);
            }
            return node;
        }

        private Entity GetUniqueValueEntity(INodeTypeLinked type, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey(type.UniqueValueFieldName))
            {
                var value = properties[type.UniqueValueFieldName].ToString();
                return _ontologyService.GetNodeByUniqueValue(type.Id, value, type.UniqueValueFieldName) as Entity;
            }
            return null;
        }

        private async Task<IEnumerable<Relation>> CreateRelations(Guid entityId,
            INodeTypeLinked embed,
            object value,
            string dotName,
            Guid requestId,
            User currentUser)
        {
            return embed.IsMultiple ?
                await CreateMultipleProperties(entityId, embed, value, string.Empty, dotName, requestId, currentUser) :
                new[] { await CreateSinglePropertyAsync(entityId, embed, value, string.Empty, dotName, requestId, currentUser) };
        }

        private void SetCreatedByIfExists(Entity entity, User currentUser)
        {
            var propertyName = "createdBy";
            var currentUserId = currentUser?.Id;
            if (currentUserId.HasValue && entity.Type.GetProperty(propertyName) != null)
            {
                entity.SetProperty(propertyName, currentUserId.Value.ToString("N"));
            }
        }

        private void SetLastConfirmedAtIfExists(Entity entity)
        {
            var propertyName = "lastConfirmedAt";
            if (entity.Type.GetProperty(propertyName) != null && entity.GetProperty(propertyName) == null)
            {
                entity.SetProperty(propertyName, DateTime.UtcNow);
            }
        }

        private async Task<Node> CreateNode(Guid entityId,
            INodeTypeLinked embed,
            object value,
            object oldValue,
            string dotName,
            Guid requestId,
            User currentUser)
        {
            var node = await CreateNode(entityId, embed, value, dotName, requestId, currentUser);

            var username = currentUser?.UserName ?? "system";
            var parentTypeName = embed.RelationType.SourceType.Name;

            var newValue = value is Dictionary<string, object> dict && dict.ContainsKey("targetId") ?
                Guid.Parse((string)dict["targetId"]) : value;

            await _changeHistoryService.SaveNodeChange(dotName,
                entityId,
                username,
                oldValue,
                newValue,
                parentTypeName,
                requestId);

            if (embed.IsLinkFromEventToObjectOfStudy)
            {
                await _changeHistoryService.SaveNodeChange(
                    "EventLink",
                    Guid.Parse(newValue.ToString()),
                    username,
                    null,
                    entityId.ToString("N"),
                    null,
                    requestId);
            }

            return node;
        }

        private async Task<Node> CreateNode(
            Guid entityId,
            INodeTypeLinked embed,
            object value,
            string dotName,
            Guid requestId,
            User currentUser) // attribute or entity
        {
            if (embed.IsAttributeType)
            {
                var scalarType = embed.TargetType.AttributeType.ScalarType;
                if (scalarType == ScalarType.File)
                    value = await InputTypesExtensions.ProcessFileInput(_fileService, value);
                else if (scalarType == ScalarType.Geo)
                    value = InputTypesExtensions.ProcessGeoInput(value);
                else
                    //All date time typw should be converted to string with time zone declaration
                    if (scalarType == ScalarType.Date)
                        value = InputTypesExtensions.ParseWithTimeZoneDefinition(value);
                    // All non-string types are converted to string before ParseValue. Numbers and booleans can be processed without it.
                    value = AttributeType.ParseValue(value.ToString(), scalarType);

                return new Attribute(Guid.NewGuid(), embed.AttributeTypeModel, value);
            }

            if (embed.IsEntityType)
            {
                var props = (Dictionary<string, object>)value;
                if (props.TryGetValue("targetId", out var strTargetId))
                {
                    var targetId = InputTypesExtensions.ParseGuid(strTargetId);
                    return _ontologyService.LoadNodeOfType(targetId, embed.TargetType);
                }

                if (props.TryGetValue("target", out var dictTarget))
                {
                    var (typeName, unionData) = InputTypesExtensions.ParseInputUnion(dictTarget);
                    var type = _ontologySchema.GetEntityTypeByName(typeName);
                    return await CreateEntity(entityId, type, unionData, dotName, Guid.NewGuid(), requestId, currentUser);
                }

                throw new ArgumentException("Incorrect arguments for Entity creation. ");
            }

            throw new ArgumentException(nameof(embed));
        }

        private async Task PutLinkedMaterialsToElasticAsync(Guid nodeId)
        {
            var signIds = _ontologyService.GetSignIds(nodeId);
            await _materialElasticService.PutMaterialsToElasticByNodeIdsAsync(signIds);
        }


    }
}
