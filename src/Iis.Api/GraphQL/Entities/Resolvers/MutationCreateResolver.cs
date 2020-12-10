using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Domain;
using Attribute = Iis.Domain.Attribute;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using Iis.Api.BackgroundServices;
using MediatR;
using Iis.Events.Entities;
using Iis.Services.Contracts;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationCreateResolver
    {
        private readonly IFileService _fileService;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMediator _mediator;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyModel _ontology;
        private readonly IResolverContext _resolverContext;

        public MutationCreateResolver(IOntologyModel ontology, 
            IOntologyService ontologyService,
            IFileService fileService,
            IChangeHistoryService changeHistoryService,
            IMediator mediator)
        {
            _ontology = ontology;
            _ontologyService = ontologyService;
            _fileService = fileService;
            _changeHistoryService = changeHistoryService;
            _mediator = mediator;            
        }

        public MutationCreateResolver(IResolverContext ctx)
        {
            _fileService = ctx.Service<IFileService>();
            _ontology = ctx.Service<IOntologyModel>();
            _ontologyService = ctx.Service<IOntologyService>();
            _changeHistoryService = ctx.Service<IChangeHistoryService>();
            _mediator = ctx.Service<IMediator>();
            _resolverContext = ctx;
        }

        public async Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _ontology.GetEntityType(typeName);
            var entity = await CreateEntity(type, data);
            return entity;
        }

        private void TrySetCreatedBy(Entity entity)
        {
            try
            {
                var currentUserId = GetCurrentUser()?.Id;
                if (currentUserId.HasValue)
                {
                    entity.SetProperty("createdBy", currentUserId.Value.ToString("N"));
                }
            }
            catch (ArgumentException) { }
        }

        public async Task<Entity> CreateEntity(
            IEntityTypeModel type, 
            Dictionary<string, object> properties)
        {
            if (properties == null)
                throw new ArgumentException($"{type.Name} creation ex nihilo is allowed only to God.");
            Entity node;
            if (type.HasUniqueValues)
            {
                node = await GetUniqueValueEntity(type, properties);
                if (node != null) return node;
            }

            var requestId = Guid.NewGuid();
            node = new Entity(Guid.NewGuid(), type);
            await CreateProperties(node, properties, requestId);
            TrySetCreatedBy(node);
            _ontologyService.SaveNode(node);
            await _mediator.Publish(new EntityCreatedEvent() { Type = type.Name, Id = node.Id });
            return node;
        }

        private async Task<Entity> CreateProperties(Entity node, 
            Dictionary<string, object> properties,
            Guid requestId)
        {
            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                var relations = await CreateRelations(node.Id, embed, value, requestId);
                foreach (var relation in relations)
                    node.AddNode(relation);
            }
            return node;
        }
        private async Task<Entity> GetUniqueValueEntity(IEntityTypeModel type, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey(type.UniqueValueFieldName))
            {
                var value = properties[type.UniqueValueFieldName].ToString();
                return (Entity)_ontologyService.GetNodeByUniqueValue(type.Id, value, type.UniqueValueFieldName);
            }
            return null;
        }

        public async Task<IEnumerable<Relation>> CreateRelations(Guid entityId, 
            IEmbeddingRelationTypeModel embed, 
            object value,
            Guid requestId)
        {
            switch (embed.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                case EmbeddingOptions.Required:
                    return new[] {await CreateSinglePropertyAsync(entityId, embed, value, requestId)};
                case EmbeddingOptions.Multiple:
                    return await CreateMultipleProperties(entityId, embed, value, requestId);
                default:
                    throw new NotImplementedException();
            }
        }

        public Task<Relation[]> CreateMultipleProperties(
            Guid entityId,
            IEmbeddingRelationTypeModel embed, 
            object value,
            Guid requestId)
        {
            var values = (IEnumerable<object>)value;
            var result = new List<Task<Relation>>();
            foreach (var v in values)
                if (embed.IsEntityType)
                {
                    result.Add(CreateSinglePropertyAsync(entityId, embed, v, requestId));
                }
                else
                {
                    var dict = (Dictionary<string, object>)v;
                    var attrValue = dict["value"];
                    result.Add(CreateSinglePropertyAsync(entityId, embed, attrValue, requestId));
                }

            return Task.WhenAll(result);
        }

        public Task<Relation[]> CreateMultipleProperties(IEmbeddingRelationTypeModel embed, object value)
        {
            var values = (IEnumerable<object>) value;
            var result = new List<Task<Relation>>();
            foreach (var v in values)
                if (embed.IsEntityType)
                {
                    result.Add(CreateSinglePropertyAsync(embed, v));
                }
                else
                {
                    var dict = (Dictionary<string, object>) v;
                    var attrValue = dict["value"];
                    result.Add(CreateSinglePropertyAsync(embed, attrValue));
                }

            return Task.WhenAll(result);
        }

        public async Task<Relation> CreateSinglePropertyAsync(
            Guid entityId,
            IEmbeddingRelationTypeModel embed, 
            object value,
            Guid requestId)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(entityId, embed, value, requestId);
            prop.AddNode(target);
            return prop;
        }

        public async Task<Relation> CreateSinglePropertyAsync(
            IEmbeddingRelationTypeModel embed,
            object value)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(embed, value);
            prop.AddNode(target);
            return prop;
        }

        public async Task<Node> CreateNode(Guid entityId, 
            IEmbeddingRelationTypeModel embed, 
            object value, 
            Guid requestId)
        {
            var node = await CreateNode(embed, value);
            if (embed.IsAttributeType)
            {
                var username = GetCurrentUser()?.UserName ?? "system";
                
                await _changeHistoryService.SaveNodeChange(embed.GetFieldName(), 
                    entityId, 
                    username, 
                    string.Empty, 
                    value.ToString(), 
                    requestId);
            }
            return node;
        } 


        public async Task<Node> CreateNode(IEmbeddingRelationTypeModel embed, object value) // attribute or entity
        {
            if (embed.IsAttributeType)
            {
                if (embed.AttributeType.ScalarTypeEnum == ScalarType.File)
                    value = await InputExtensions.ProcessFileInput(_fileService, value);
                else if (embed.AttributeType.ScalarTypeEnum == ScalarType.Geo)
                    value = InputExtensions.ProcessGeoInput(value);
                else
                    // All non-string types are converted to string before ParseValue. Numbers and booleans can be processed without it.
                    value = AttributeType.ParseValue(value.ToString(), embed.AttributeType.ScalarTypeEnum);
                
                return new Attribute(Guid.NewGuid(), embed.AttributeType, value);
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
                    var type = _ontology.GetEntityType(typeName);
                    return await CreateEntity(type, unionData);
                }

                throw new ArgumentException("Incorrect arguments for Entity creation. ");
            }

            throw new ArgumentException(nameof(embed));
         }

        private User GetCurrentUser()
        {
            if (_resolverContext is null) return null;

            var tokenPayload = _resolverContext.ContextData["token"] as TokenPayload;
            return tokenPayload?.User;
        }
    }

    
}
