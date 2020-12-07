using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Domain;
using Attribute = Iis.Domain.Attribute;
using IIS.Domain;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;
using Iis.Api.BackgroundServices;
using MediatR;
using Iis.Events.Entities;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationUpdateResolver
    {
        private readonly MutationCreateResolver _mutationCreateResolver;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyModel _ontology;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMediator _mediator;
        private readonly IResolverContext _resolverContext;
        private Guid _rootNodeId;
        private const string LastConfirmedFieldName = "lastConfirmedAt";

        public MutationUpdateResolver(IOntologyService ontologyService,
            IOntologyModel ontology,
            IChangeHistoryService changeHistoryService,
            IMediator mediator,
            MutationCreateResolver mutationCreateResolver)
        {
            _mutationCreateResolver = mutationCreateResolver;
            _ontology = ontology;
            _ontologyService = ontologyService;
            _changeHistoryService = changeHistoryService;
            _mediator = mediator;
        }
        public MutationUpdateResolver(IResolverContext ctx)
        {
            _mutationCreateResolver = new MutationCreateResolver(ctx);
            _ontology = ctx.Service<IOntologyModel>();
            _ontologyService = ctx.Service<IOntologyService>();
            _changeHistoryService = ctx.Service<IChangeHistoryService>();
            _mediator = ctx.Service<IMediator>();
            _resolverContext = ctx;

        }

        public async Task<Entity> UpdateEntity(IResolverContext ctx, string typeName)
        {
            var id = ctx.Argument<Guid>("id");
            var data = ctx.Argument<Dictionary<string, object>>("data");
            if (!data.ContainsKey(LastConfirmedFieldName))
                data.Add(LastConfirmedFieldName, DateTime.UtcNow);

            var type = _ontology.GetEntityType(typeName);
            _rootNodeId = id;
            var requestId = Guid.NewGuid();
            return await UpdateEntity(type, id, data, string.Empty, requestId);
        }
        public Task<Entity> UpdateEntity(IEntityTypeModel type, Guid id, Dictionary<string, object> properties)
        {
            _rootNodeId = id;
            var requestId = Guid.NewGuid();

            return UpdateEntity(type, id, properties, string.Empty, requestId);
        }
        private async Task<Entity> UpdateEntity(
            IEntityTypeModel type, Guid id,
            Dictionary<string, object> properties, string dotName, Guid requestId)
        {
            var node = (Entity)_ontologyService.LoadNodes(id);
            if (node == null)
                throw new ArgumentException($"There is no entity with id {id}");
            if (!type.IsAssignableFrom(node.Type)) // no direct checking of types - we can update child as its base type
                throw new ArgumentException($"Type {node.Type.Name} can not be updated as {type.Name}");

            if (type.HasUniqueValues && properties.ContainsKey(type.UniqueValueFieldName) && node.GetProperty(type.UniqueValueFieldName) != null)
            {
                var newNode = await _mutationCreateResolver.CreateEntity(type, properties);
                if (newNode.Id != node.Id)
                {
                    node = (Entity)_ontologyService.LoadNodes(newNode.Id);
                }
            }
            else
            {
                foreach (var (key, value) in properties)
                {
                    var embed = node.Type.GetProperty(key);
                    if (embed == null)
                    {
                        if (key == LastConfirmedFieldName)
                            continue;
                        else
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                    }

                    await UpdateRelations(node, embed, value,
                        string.IsNullOrEmpty(dotName) ? key : dotName + "." + key, requestId);
                }
                _ontologyService.SaveNode(node);
            }
            await _mediator.Publish(new EntityUpdatedEvent
            {
                Id = node.Id,
                Type = type.Name,
                RequestId = requestId
            });
            return node;
        }

        protected virtual async Task UpdateRelations(
            Node node, IEmbeddingRelationTypeModel embed,
            object value, string dotName, Guid requestId)
        {
            switch (embed.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                case EmbeddingOptions.Required:
                    await UpdateSingleProperty(node, embed, value, dotName, requestId);
                    break;
                case EmbeddingOptions.Multiple:
                    await UpdateMultipleProperties(node, embed, value, dotName, requestId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(embed));
            }
        }

        protected virtual async Task UpdateMultipleProperties(
            Node node, IEmbeddingRelationTypeModel embed,
            object value, string dotName, Guid requestId)
        {
            // patch input on multiple property
            var patch = (Dictionary<string, object>)value;
            foreach (var (key, v) in patch)
            {
                var list = (IEnumerable<object>)v;
                switch (key)
                {
                    case "create":
                        var relations = await _mutationCreateResolver.CreateMultipleProperties(embed, v);
                        foreach (var relation in relations)
                        {
                            node.AddNode(relation);
                            await SaveChangesForNewRelation(relation, requestId);
                        }
                        break;
                    case "update":
                        foreach (var uv in list)
                            await ApplyUpdate(node, embed, uv, dotName, requestId);

                        break;
                    case "delete":
                        foreach (var dv in list)
                        {
                            var id = InputExtensions.ParseGuid(dv);
                            var relation = node.GetRelation(embed, id);
                            node.RemoveNode(relation);
                            await SaveChangesForDeletedRelation(relation, requestId);
                        }

                        break;
                    default:
                        throw new ArgumentException($"Unknown patch object property: {key}");
                }
            }
        }

        protected async Task ApplyUpdate(Node node, IEmbeddingRelationTypeModel embed, object uv, string dotName, Guid requestId)
        {
            var uvdict = (Dictionary<string, object>)uv; // RelationTo_U_Entity
            Relation relation;
            if (embed.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                var relationId = InputExtensions.ParseGuid(uvdict["id"]); // this is NOT target entity id, but relation id
                relation = node.GetRelation(embed, relationId);
            }
            else
            {
                relation = node.GetRelation(embed);
            }

            if (embed.IsEntityType)
            {
                if (uvdict.ContainsKey("target"))
                {
                    var (typeName, targetValue) = InputExtensions.ParseInputUnion(uvdict["target"]);
                    var type = _ontology.GetEntityType(typeName);
                    var updatedNode = await UpdateEntity(type, relation.Target.Id, targetValue, dotName, requestId);
                    if (relation.Target.Id != updatedNode.Id)
                    {
                        node.RemoveNode(relation);
                        var newRelation = new Relation(Guid.NewGuid(), embed);
                        var target = (Entity)_ontologyService.LoadNodes(updatedNode.Id);
                        newRelation.AddNode(target);
                        node.AddNode(newRelation);
                    }
                }
                else // targetId only
                {
                    node.RemoveNode(relation);
                    var newRel = await _mutationCreateResolver.CreateSingleProperty(embed, uvdict);
                    node.AddNode(newRel);
                }
            }
            else if (embed.IsAttributeType)
            {
                var attrvalue = uvdict["value"];
                await UpdateSingleProperty(node, embed, attrvalue, relation, dotName, requestId);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual async Task UpdateSingleProperty(
            Node node, IEmbeddingRelationTypeModel embed,
            object value, string dotName, Guid requestId)
        {
            var existingRelation = node.GetRelationOrDefault(embed);
            if (value == null || embed.IsAttributeType) // skip parsing internal structure
            {
                await UpdateSingleProperty(node, embed, value, existingRelation, dotName, requestId);
                return;
            }
            var patch = (Dictionary<string, object>)value;
            if (patch.Count != 1)
                throw new ArgumentException($"Expected to find exactly one element at Single Patch input '{embed.Name}'");
            var (action, inputObject) = patch.Single();
            switch (action)
            {
                case "target":
                case "targetId":
                    await UpdateSingleProperty(node, embed, patch, existingRelation, dotName, requestId); // pass full object to CreateSingleProperty
                    break;
                case "create":
                    await UpdateSingleProperty(node, embed, inputObject, existingRelation, dotName, requestId);
                    break;
                case "update":
                    await ApplyUpdate(node, embed, inputObject, dotName, requestId);
                    break;
                case "delete":
                    node.RemoveNode(existingRelation);
                    await SaveChangesForDeletedRelation(existingRelation, requestId);
                    break;
                default:
                    throw new ArgumentException($"Unknown single patch object property: {action}");
            }
        }

        protected virtual async Task UpdateSingleProperty(Node node, IEmbeddingRelationTypeModel embed, object value,
            Relation oldRelation, string dotName, Guid requestId)
        {
            if (oldRelation != null)
            {
                node.RemoveNode(oldRelation);
                if (value == null)
                {
                    await SaveChangesForDeletedRelation(oldRelation, requestId);
                }
            }

            if (value != null)
            {
                var stringifiedValue = value is string ? (string)value : JsonConvert.SerializeObject(value);
                var newRelation = await _mutationCreateResolver.CreateSingleProperty(embed, value);
                node.AddNode(newRelation);
                if (oldRelation == null)
                {
                    if (newRelation.Target is Attribute)
                    {
                        await _changeHistoryService
                            .SaveNodeChange(dotName, _rootNodeId, GetCurrentUserName(), string.Empty, stringifiedValue, requestId);
                    }
                    else
                    {
                        await SaveChangesForNewRelation(newRelation, requestId);
                    }
                }
                else if (oldRelation.Target is Attribute)
                {
                    var oldValueObj = (oldRelation.Target as Attribute).Value;
                    var oldValue = oldValueObj is string ? (string)oldValueObj : JsonConvert.SerializeObject(oldValueObj);

                    if (oldValue != stringifiedValue)
                    {
                        await _changeHistoryService
                            .SaveNodeChange(dotName, _rootNodeId, GetCurrentUserName(), oldValue, stringifiedValue, requestId);
                    }
                }
            }
        }

        private string GetCurrentUserName()
        {
            if (_resolverContext is null) return "system";

            var tokenPayload = _resolverContext.ContextData["token"] as TokenPayload;
            return tokenPayload?.User?.UserName;
        }

        private async Task SaveChangesForNewRelation(Relation relation, Guid requestId)
        {
            var children = relation.GetChildAttributes();
            foreach (var child in children)
            {
                await _changeHistoryService
                    .SaveNodeChange(child.dotName, _rootNodeId,
                        GetCurrentUserName(), string.Empty, child.attribute.Value?.ToString(), requestId);
            }
        }

        private async Task SaveChangesForDeletedRelation(Relation relation, Guid requestId)
        {
            var children = relation.GetChildAttributes();
            foreach (var child in children)
            {
                await _changeHistoryService
                    .SaveNodeChange(child.dotName, _rootNodeId,
                        GetCurrentUserName(), child.attribute.Value.ToString(), string.Empty, requestId);
            }
        }
    }
}
