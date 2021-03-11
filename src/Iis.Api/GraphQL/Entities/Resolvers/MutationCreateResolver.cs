using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Domain;
using Attribute = Iis.Domain.Attribute;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using Iis.Events.Entities;
using Iis.Services.Contracts;
using Newtonsoft.Json;
using Iis.OntologySchema.DataTypes;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationCreateResolver
    {
        private readonly IFileService _fileService;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMediator _mediator;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IResolverContext _resolverContext;

        public MutationCreateResolver(IOntologySchema ontologySchema, 
            IOntologyService ontologyService,
            IFileService fileService,
            IChangeHistoryService changeHistoryService,
            IMediator mediator)
        {
            _ontologySchema = ontologySchema;
            _ontologyService = ontologyService;
            _fileService = fileService;
            _changeHistoryService = changeHistoryService;
            _mediator = mediator;
        }

        public MutationCreateResolver(IResolverContext ctx)
        {
            _fileService = ctx.Service<IFileService>();
            _ontologySchema = ctx.Service<IOntologySchema>();
            _ontologyService = ctx.Service<IOntologyService>();
            _changeHistoryService = ctx.Service<IChangeHistoryService>();
            _mediator = ctx.Service<IMediator>();
            _resolverContext = ctx;
        }

        public async Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _ontologySchema.GetEntityTypeByName(typeName);            
            var entity = await CreateRootEntity(Guid.NewGuid(), type, data);
            return entity;
        }

        private void SetCreatedByIfExists(Entity entity)
        {
            var propertyName = "createdBy";
            var currentUserId = GetCurrentUser()?.Id;
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

        private Task<Entity> CreateRootEntity(
            Guid entityId,
            INodeTypeLinked type,
            Dictionary<string, object> properties)
        {
            return CreateEntityCore(entityId, type, properties, string.Empty, entityId, Guid.NewGuid());
        }

        public Task<Entity> CreateEntity(
            Guid entityId,
            INodeTypeLinked type,
            string dotName,
            Dictionary<string, object> properties,
            Guid requestId)
        {
            return CreateEntityCore(entityId, type, properties, dotName, Guid.NewGuid(), requestId);
        }

        private async Task<Entity> CreateEntityCore(Guid rootEntityId,
            INodeTypeLinked type, 
            Dictionary<string, object> properties,
            string dotName,
            Guid entityId,
            Guid requestId)
        {
            if (properties == null)
                throw new ArgumentException($"{type.Name} creation ex nihilo is allowed only to God.");
            Entity node;
            if (type.HasUniqueValues)
            {
                node = GetUniqueValueEntity(type, properties);
                if (node != null) return node;
            }

            node = new Entity(entityId, type);
            await CreateProperties(rootEntityId, node, properties, dotName, requestId);
            SetCreatedByIfExists(node);
            SetLastConfirmedAtIfExists(node);
            _ontologyService.SaveNode(node);
            await _mediator.Publish(new EntityCreatedEvent() { Type = type.Name, Id = node.Id });
            return node;
        }

        private async Task<Entity> CreateProperties(Guid entityId,
            Entity node, 
            Dictionary<string, object> properties,
            string dotName,
            Guid requestId)
        {
            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                var newDotName = string.IsNullOrEmpty(dotName)
                    ? key
                    : $"{dotName}.{key}";
                var relations = await CreateRelations(entityId, embed, value, newDotName, requestId);
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
            Guid requestId)
        {
            switch (embed.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                case EmbeddingOptions.Required:
                    return new[] {await CreateSinglePropertyAsync(entityId, embed, value, string.Empty, dotName, requestId)};
                case EmbeddingOptions.Multiple:
                    return await CreateMultipleProperties(entityId, embed, value, string.Empty, dotName, requestId);
                default:
                    throw new NotImplementedException();
            }
        }

        public Task<Relation[]> CreateMultipleProperties(
            Guid entityId,
            INodeTypeLinked embed, 
            object value,
            string oldValue,
            string dotName,
            Guid requestId)
        {
            var values = (IEnumerable<object>)value;
            var result = new List<Task<Relation>>();
            foreach (var v in values)
                if (embed.IsEntityType)
                {
                    result.Add(CreateSinglePropertyAsync(entityId, embed, v, oldValue, dotName, requestId));
                }
                else
                {
                    var dict = (Dictionary<string, object>)v;
                    var attrValue = dict["value"];
                    result.Add(CreateSinglePropertyAsync(entityId, embed, attrValue, oldValue, dotName, requestId));
                }

            return Task.WhenAll(result);
        }

        public async Task<Relation> CreateSinglePropertyAsync(
            Guid entityId,
            INodeTypeLinked embed, 
            object value,
            object oldValue,
            string dotName,
            Guid requestId)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(entityId, embed, value, oldValue, dotName, requestId);
            prop.AddNode(target);
            return prop;
        }

        private async Task<Node> CreateNode(Guid entityId,
            INodeTypeLinked embed,             
            object value, 
            object oldValue,
            string dotName,
            Guid requestId)
        {
            var node = await CreateNode(entityId, embed, value, dotName, requestId);
            
            var username = GetCurrentUser()?.UserName ?? "system";
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
            Guid requestId) // attribute or entity
        {
            if (embed.IsAttributeType)
            {
                var scalarType = embed.TargetType.AttributeType.ScalarType;
                if (scalarType == ScalarType.File)
                    value = await InputExtensions.ProcessFileInput(_fileService, value);
                else if (scalarType == ScalarType.Geo)
                    value = InputExtensions.ProcessGeoInput(value);
                else
                    // All non-string types are converted to string before ParseValue. Numbers and booleans can be processed without it.
                    value = AttributeType.ParseValue(value.ToString(), scalarType);
                
                return new Attribute(Guid.NewGuid(), embed.AttributeTypeModel, value);
            }

            if (embed.IsEntityType)
            {
                var props = (Dictionary<string, object>) value;
                if (props.TryGetValue("targetId", out var strTargetId))
                {
                    var targetId = InputExtensions.ParseGuid(strTargetId);
                    return _ontologyService.LoadNodeOfType(targetId, embed.TargetType);
                }

                if (props.TryGetValue("target", out var dictTarget))
                {
                    var (typeName, unionData) = InputExtensions.ParseInputUnion(dictTarget);
                    var type = _ontologySchema.GetEntityTypeByName(typeName);
                    return await CreateEntity(entityId, type, dotName, unionData, requestId);
                }

                throw new ArgumentException("Incorrect arguments for Entity creation. ");
            }

            throw new ArgumentException(nameof(embed));
         }

        private User GetCurrentUser()
        {
            if (_resolverContext is null) return null;

            var tokenPayload = _resolverContext.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;
            return tokenPayload?.User;
        }
    }
}
