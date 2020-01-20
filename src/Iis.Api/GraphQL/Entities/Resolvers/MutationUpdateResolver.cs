using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;
using Attribute = Iis.Domain.Attribute;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class MutationUpdateResolver
    {
        private readonly MutationCreateResolver _mutationCreateResolver;
        private readonly MutationDeleteResolver _mutationDeleteResolver;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyProvider _ontologyProvider;

        public MutationUpdateResolver(MutationCreateResolver mutationCreateResolver,
            MutationDeleteResolver mutationDeleteResolver, IOntologyProvider ontologyProvider,
            IOntologyService ontologyService)
        {
            _mutationCreateResolver = mutationCreateResolver;
            _mutationDeleteResolver = mutationDeleteResolver;
            _ontologyProvider = ontologyProvider;
            _ontologyService = ontologyService;
        }

        public MutationUpdateResolver(IResolverContext ctx)
        {
            _mutationCreateResolver = new MutationCreateResolver(ctx);
            _mutationDeleteResolver = new MutationDeleteResolver(ctx);
            _ontologyProvider = ctx.Service<IOntologyProvider>();
            _ontologyService = ctx.Service<IOntologyService>();
        }

        public async Task<Entity> UpdateEntity(IResolverContext ctx, string typeName)
        {
            var id = ctx.Argument<Guid>("id");
            var data = ctx.Argument<Dictionary<string, object>>("data");

            var ontology = await _ontologyProvider.GetOntologyAsync();
            var type = ontology.GetEntityType(typeName);
            return await UpdateEntity(type, id, data);
        }

        private async Task<Entity> UpdateEntity(EntityType type, Guid id, Dictionary<string, object> properties)
        {
            var node = (Entity) await _ontologyService.LoadNodesAsync(id, null);
            if (node == null)
                throw new ArgumentException($"There is no entity with id {id}");
            if (!type.IsAssignableFrom(node.Type)) // no direct checking of types - we can update child as its base type
                throw new ArgumentException($"Type {node.Type.Name} can not be updated as {type.Name}");

            foreach (var (key, value) in properties)
            {
                var embed = node.Type.GetProperty(key) ??
                            throw new ArgumentException($"There is no property '{key}' on type '{node.Type.Name}'");
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
                    await UpdateSingleProperty(node, embed, value);
                    break;
                case EmbeddingOptions.Multiple:
                    await UpdateMultipleProperties(node, embed, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(embed));
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
                        var relations = await _mutationCreateResolver.CreateMultipleProperties(embed, v);
                        foreach (var r in relations)
                            node.AddNode(r);
                        break;
                    case "update":
                        foreach (var uv in list)
                            await ApplyUpdate(node, embed, uv);

                        break;
                    case "delete":
                        foreach (var dv in list)
                        {
                            var id = InputExtensions.ParseGuid(dv);
                            var relation = node.GetRelation(embed, id);
                            node.RemoveNode(relation);
                        }

                        break;
                    default:
                        throw new ArgumentException($"Unknown patch object property: {key}");
                }
            }
        }

        protected async Task ApplyUpdate(Node node, EmbeddingRelationType embed, object uv)
        {
            var uvdict = (Dictionary<string, object>) uv; // RelationTo_U_Entity
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
                    var ontology = await _ontologyProvider.GetOntologyAsync();
                    var type = ontology.GetEntityType(typeName);
                    await UpdateEntity(type, relation.Target.Id, targetValue);
                }
                else // targetId only
                {
                    // todo: look at it again and refactor
                    var targetId = InputExtensions.ParseGuid(uvdict["targetId"]); // just check
//                    var targetRelation = node.GetRelation(embed, relation.Id);
                    node.RemoveNode(relation);
                    var newRel = await _mutationCreateResolver.CreateSingleProperty(embed, uvdict);
                    node.AddNode(newRel);
                }
            }
            else if (embed.IsAttributeType)
            {
                var attrvalue = uvdict["value"];
                await UpdateSingleProperty(node, embed, attrvalue, relation);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual async Task UpdateSingleProperty(Node node, EmbeddingRelationType embed, object value)
        {
            var existingRelation = node.GetRelationOrDefault(embed);
            if (value == null || embed.IsAttributeType) // skip parsing internal structure
            {
                await UpdateSingleProperty(node, embed, value, existingRelation);
                return;
            }
            var patch = (Dictionary<string, object>) value;
            if (patch.Count != 1)
                throw new ArgumentException($"Expected to find exactly one element at Single Patch input '{embed.Name}'");
            var (action, inputObject) = patch.Single();
            switch (action)
            {
                case "target":
                case "targetId":
                    await UpdateSingleProperty(node, embed, patch, existingRelation); // pass full object to CreateSingleProperty
                    break;
                case "create":
                    await UpdateSingleProperty(node, embed, inputObject, existingRelation);
                    break;
                case "update":
                    await ApplyUpdate(node, embed, inputObject);
                    break;
                default:
                    throw new ArgumentException($"Unknown single patch object property: {action}");
            }
        }

        protected virtual async Task UpdateSingleProperty(Node node, EmbeddingRelationType embed, object value,
            Relation relation)
        {
            if (relation != null)
                node.RemoveNode(relation);
            if (value != null)
                node.AddNode(await _mutationCreateResolver.CreateSingleProperty(embed, value));
        }
    }
}
