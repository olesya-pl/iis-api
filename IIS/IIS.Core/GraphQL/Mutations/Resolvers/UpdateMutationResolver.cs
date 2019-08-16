using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Mutations.Resolvers
{
    public class UpdateMutationResolver
    {
        private readonly CreateMutationResolver _createMutationResolver;
        private readonly DeleteMutationResolver _deleteMutationResolver;
        private readonly IOntologyRepository _repository;
        private readonly IOntologyService _ontologyService;

        public UpdateMutationResolver(CreateMutationResolver createMutationResolver, DeleteMutationResolver deleteMutationResolver, IOntologyRepository repository, IOntologyService ontologyService)
        {
            _createMutationResolver = createMutationResolver;
            _deleteMutationResolver = deleteMutationResolver;
            _repository = repository;
            _ontologyService = ontologyService;
        }

        public UpdateMutationResolver(IResolverContext ctx)
        {
            _createMutationResolver = new CreateMutationResolver(ctx);
            _deleteMutationResolver = new DeleteMutationResolver(ctx);
            _repository = ctx.Service<IOntologyRepository>();
            _ontologyService = ctx.Service<IOntologyService>();
        }

        public async Task<Entity> UpdateEntity(IResolverContext ctx, string typeName)
        {
            var id = ctx.Argument<Guid>("id");
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var type = _repository.GetEntityType(typeName);
            return await UpdateEntity(type, id, data);
        }

        private async Task<Entity> UpdateEntity(EntityType type, Guid id, Dictionary<string, object> properties)
        {
            var node = (Entity) await _ontologyService.LoadNodesAsync(id, null);
            if (!type.IsAssignableFrom(node.Type)) // no direct checking of types - we can update child as its base type
                throw new ArgumentException($"Type {node.Type.Name} can not be updated as {type.Name}");

            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ?? throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
                await UpdateRelations(node, embed, value);
            }

            await _ontologyService.SaveNodeAsync(node);
            return node;
        }

        protected virtual async Task UpdateRelations(Node node, EmbeddingRelationType embed, object value)
        {
            switch (embed.EmbeddingOptions)
            {
                case EmbeddingOptions.Optional:
                case EmbeddingOptions.Required:
                    await UpdateSingleProperty(node, embed, value, node.GetRelation(embed));
                    break;
                case EmbeddingOptions.Multiple:
                    await UpdateMultipleProperties(node, embed, value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual async Task UpdateMultipleProperties(Node node, EmbeddingRelationType embed, object value)
        {
            // patch input on multiple property
            var patch = (Dictionary<string, object>) value;
            foreach (var (key, v) in patch)
            {
                var list = (IEnumerable<object>) v;
                switch (key)
                {
                    case "create":
                        var relations = await _createMutationResolver.CreateMultipleProperties(embed, v);
                        foreach (var r in relations)
                            node.AddNode(r);
                        break;
                    case "update":
                        foreach (var uv in list)
                        {
                            var uvdict = (Dictionary<string, object>) uv; // RelationTo_U_Entity
                            var id = InputExtensions.ParseGuid(uvdict["id"]); // considering this is an target entity id
                            var relation = node.GetRelation(embed, id);
                            if (relation == null)
                                throw new Exception($"There is no relation from {node.Type} {node.Id} to {embed.Name} of type {embed.TargetType} with id {id}");
                            if (embed.IsEntityType)
                            {
                                var (typeName, targetValue) = InputExtensions.ParseInputUnion(uvdict["target"]);
                                var type = _repository.GetEntityType(typeName);
                                await UpdateEntity(type, id, targetValue);
                            }
                            else if (embed.IsAttributeType)
                            {
                                var attrvalue = uvdict["value"];
                                await UpdateSingleProperty(node, embed, attrvalue, relation);
                            }
                            else throw new ArgumentOutOfRangeException();
                            break;
                        }
                        break;
                    case "delete":
                        foreach (var dv in list)
                        {
                            var id = InputExtensions.ParseGuid(dv);
                            // todo: maybe we should delete only relation? Or relation with entity?
                            await _deleteMutationResolver.DeleteEntity(id, embed.TargetType.Name);
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown patch object property: {key}");
                }
            }
        }

        protected virtual async Task UpdateSingleProperty(Node node, EmbeddingRelationType embed, object value, Relation relation)
        {
            if (relation == null)
            {
                node.AddNode(await _createMutationResolver.CreateSingleProperty(embed, value));
            }
            else
            {
                relation.RemoveNode(relation.Target);
                var newTarget = await _createMutationResolver.CreateNode(embed, value);
                relation.AddNode(newTarget);
            }
        }

        protected virtual async Task<Node> CreateNode(EmbeddingRelationType embed, object value) // attribute or entity
        {
            if (embed.IsAttributeType)
                return new IIS.Core.Ontology.Attribute(Guid.NewGuid(), embed.AttributeType, value);
            if (embed.IsEntityType)
            {
                var props = (Dictionary<string, object>) value;
                if (props.TryGetValue("targetId", out var strTargetId))
                {
                    var targetId = Guid.Parse((string) strTargetId);
                    return await _ontologyService.LoadNodeOfType(targetId, embed.TargetType);
                }

                if (props.TryGetValue("target", out var dictTarget))
                {
                    var target = (Dictionary<string, object>) dictTarget;
                    var (key, v) = target.Single(); // todo: throw correct exception
                    var type = _repository.GetEntityType(key);
//                    return await CreateEntity(type, (Dictionary<string, object>) v);
                }

                throw new ArgumentException("Incorrect arguments for Entity creation. ");
            }

            throw new ArgumentException(nameof(embed));
        }
    }
}
