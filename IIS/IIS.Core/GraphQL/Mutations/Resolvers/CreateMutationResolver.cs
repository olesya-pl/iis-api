using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Files;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations.Resolvers
{
    public class CreateMutationResolver
    {
        private readonly IOntologyRepository _repository;
        private readonly IOntologyService _ontologyService;
        private readonly IFileService _fileService;

        public CreateMutationResolver(IOntologyRepository repository, IOntologyService ontologyService, IFileService fileService)
        {
            _repository = repository;
            _ontologyService = ontologyService;
            _fileService = fileService;
        }

        public CreateMutationResolver(IResolverContext ctx)
        {
            _fileService = ctx.Service<IFileService>();
            _repository = ctx.Service<IOntologyRepository>();
            _ontologyService = ctx.Service<IOntologyService>();
        }

        public async Task<Entity> CreateEntity(IResolverContext ctx, string typeName)
        {
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _repository.GetEntityType(typeName);
            var entity = await CreateEntity(type, data);

            return entity;
        }

        public async Task<Entity> CreateEntity(EntityType type, Dictionary<string, object> properties)
        {
            var node = new Entity(Guid.NewGuid(), type);
            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                var relations = await CreateRelations(embed, value);
                foreach (var relation in relations)
                    node.AddNode(relation);
            }

            await _ontologyService.SaveNodeAsync(node);
            return node;
        }

        public async Task<IEnumerable<Relation>> CreateRelations(EmbeddingRelationType embed, object value)
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

        public async Task<IEnumerable<Relation>> CreateMultipleProperties(EmbeddingRelationType embed, object value)
        {
            var values = (IEnumerable<object>) value;
            var result = new List<Relation>();
            foreach (var v in values)
            {
                if (embed.IsEntityType)
                    result.Add(await CreateSingleProperty(embed, v));
                else
                {
                    var dict = (Dictionary<string, object>) v;
                    var attrValue = dict["value"];
                    result.Add(await CreateSingleProperty(embed, attrValue));
                }
            }
            return result;
        }

        public async Task<Relation> CreateSingleProperty(EmbeddingRelationType embed, object value)
        {
            var prop = new Relation(Guid.NewGuid(), embed);
            var target = await CreateNode(embed, value);
            prop.AddNode(target);
            return prop;
        }

        public async Task<Node> CreateNode(EmbeddingRelationType embed, object value) // attribute or entity
        {
            if (embed.IsAttributeType)
            {
                if (embed.AttributeType.ScalarTypeEnum == Core.Ontology.ScalarType.File)
                    value = await InputExtensions.ProcessFileInput(_fileService, value);
                if (embed.AttributeType.ScalarTypeEnum == Core.Ontology.ScalarType.Geo)
                    value = InputExtensions.ProcessGeoInput(value);
                return new IIS.Core.Ontology.Attribute(Guid.NewGuid(), embed.AttributeType, value);
            }
            if (embed.IsEntityType)
            {
                var props = (Dictionary<string, object>) value;
                if (props.TryGetValue("targetId", out var strTargetId))
                {
                    var targetId = InputExtensions.ParseGuid(strTargetId);
                    return await _ontologyService.LoadNodeOfType(targetId, embed.TargetType);
                }

                if (props.TryGetValue("target", out var dictTarget))
                {
                    var (typeName, unionData) = InputExtensions.ParseInputUnion(dictTarget);
                    var type = _repository.GetEntityType(typeName);
                    return await CreateEntity(type, unionData);
                }

                throw new ArgumentException("Incorrect arguments for Entity creation. ");
            }

            throw new ArgumentException(nameof(embed));
        }
    }
}
