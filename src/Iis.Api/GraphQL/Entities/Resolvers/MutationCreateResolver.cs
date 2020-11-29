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

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationCreateResolver
    {
        private readonly IFileService _fileService;
        private readonly IMediator _mediator;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyModel _ontology;

        public MutationCreateResolver(IOntologyModel ontology, 
            IOntologyService ontologyService,
            IFileService fileService,
            IMediator mediator)
        {
            _ontology = ontology;
            _ontologyService = ontologyService;
            _fileService = fileService;
            _mediator = mediator;
        }

        public MutationCreateResolver(IResolverContext ctx)
        {
            _fileService = ctx.Service<IFileService>();
            _ontology = ctx.Service<IOntologyModel>();
            _ontologyService = ctx.Service<IOntologyService>();
            _mediator = ctx.Service<IMediator>();
        }

        public async Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _ontology.GetEntityType(typeName);
            var entity = await CreateEntity(type, data);

            return entity;
        }

        public async Task<Entity> CreateEntity(INodeTypeModel type, Dictionary<string, object> properties)
        {
            if (properties == null)
                throw new ArgumentException($"{type.Name} creation ex nihilo is allowed only to God.");
            Entity node;
            if (type.HasUniqueValues)
            {
                node = await GetUniqueValueEntity(type, properties);
                if (node != null) return node;
            }

            node = new Entity(Guid.NewGuid(), type);
            await CreateProperties(node, properties);
            await _ontologyService.SaveNodeAsync(node);
            await _mediator.Publish(new EntityCreatedEvent() { Type = type.Name });
            return node;
        }

        private async Task<Entity> CreateProperties(Entity node, Dictionary<string, object> properties)
        {
            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                var relations = await CreateRelations(embed, value);
                foreach (var relation in relations)
                    node.AddNode(relation);
            }
            return node;
        }
        private async Task<Entity> GetUniqueValueEntity(INodeTypeModel type, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey(type.UniqueValueFieldName))
            {
                var value = properties[type.UniqueValueFieldName].ToString();
                return (Entity)_ontologyService.GetNodeByUniqueValue(type.Id, value, type.UniqueValueFieldName);
            }
            return null;
        }

        public async Task<IEnumerable<Relation>> CreateRelations(INodeTypeModel embed, object value)
        {
            switch (embed.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                case EmbeddingOptions.Required:
                    return new[] {await CreateSingleProperty(embed, value)};
                case EmbeddingOptions.Multiple:
                    return await CreateMultipleProperties(embed, value);
                default:
                    throw new NotImplementedException();
            }
        }

        public Task<Relation[]> CreateMultipleProperties(INodeTypeModel embed, object value)
        {
            var values = (IEnumerable<object>) value;
            var result = new List<Task<Relation>>();
            foreach (var v in values)
                if (embed.IsEntityType)
                {
                    result.Add(CreateSingleProperty(embed, v));
                }
                else
                {
                    var dict = (Dictionary<string, object>) v;
                    var attrValue = dict["value"];
                    result.Add(CreateSingleProperty(embed, attrValue));
                }

            return Task.WhenAll(result);
        }

        public async Task<Relation> CreateSingleProperty(INodeTypeModel embed, object value)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(embed, value);
            prop.AddNode(target);
            return prop;
        }

        public async Task<Node> CreateNode(INodeTypeModel embed, object value) // attribute or entity
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
    }
}
