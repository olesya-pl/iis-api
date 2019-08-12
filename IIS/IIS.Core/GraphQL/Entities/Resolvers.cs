using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using Attribute = IIS.Core.Ontology.Attribute;

namespace IIS.Core.GraphQL.Entities
{
    public static class Resolvers
    {
        private const string LastRelation = "LastRelation";

        public static ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node) resolverResult;
            var typeName = OntologyObjectType.GetName(node.Type);
            return context.Schema.GetType<ObjectType>(typeName);
        }

        // ----- Root Entity resolvers ----- //

        public static async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public static async Task<Entity> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var id = ctx.Argument<Guid>("id");
            var node = await ontologyService.LoadNodesAsync(new Entity(id), null);
            return node as Entity; // ???
        }

        public static async Task<IEnumerable<Entity>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var list = await ontologyService.GetNodesByTypeAsync(type); // Direct type
            return list.OfType<Entity>();
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public static async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent, new[] { relationType });
            var relation = node.Nodes.OfType<Relation>().Single(n => n.Type.Name == relationType.Name);
            var attribute = relation.Nodes.OfType<Attribute>().Single();
            return await ResolveAttributeValue(ctx, attribute);
        }

        // resolve multiple entity-[attribute] relation
        public static async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent, new[] { relationType });
            return node.Nodes.OfType<Relation>().Where(n => n.Type.Name == relationType.Name);
        }

        // Parent - embedding relation to attribute, resolve attribute value here
        public static async Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx)
        {
            var parent = ctx.Parent<Relation>();
            var attribute = parent.Nodes.OfType<Attribute>().Single();
            return await ResolveAttributeValue(ctx, attribute);
        }

        // Return any scalar type for given attribute
        public static async Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute)
        {
            var attributeType = (AttributeType) attribute.Type;
            switch (attributeType.ScalarTypeEnum)
            {
                case Core.Ontology.ScalarType.String:
                    return attribute.Value;
                case Core.Ontology.ScalarType.Integer:
                    return attribute.Value;
                case Core.Ontology.ScalarType.Decimal:
                    return attribute.Value;
                case Core.Ontology.ScalarType.Boolean:
                    return attribute.Value;
                case Core.Ontology.ScalarType.DateTime:
                    return attribute.Value;
                case Core.Ontology.ScalarType.Geo:
                    throw new NotImplementedException();
                case Core.Ontology.ScalarType.File:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // ----- Relations to entities ----- //

        // Resolve one or multiple relations to entity. Return either Entity or IEnumerable<Entity>
        public static async Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ontologyService.LoadNodesAsync(parent, new[] { relationType });
            var relations = node.Nodes.OfType<Relation>().Where(r => r.Type.Name == relationType.Name);
            var relationsInfo = relations.ToDictionary(r => r.Nodes.OfType<Entity>().Single());
            ctx.SetRelationInfo(relationsInfo); // pass info to _relation
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                return relationsInfo.Keys;
            return relationsInfo.Keys.Single();
        }

        // resolver for "_relation" field on schema, passed through context
        public static async Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return ctx.GetRelationInfo(parent);
        }

        // ----- Context operations ----- //

        private static void SetRelationInfo(this IResolverContext ctx, Dictionary<Entity, Relation> relationsInfo)
        {
            ctx.ScopedContextData = ctx.ScopedContextData.SetItem(LastRelation, relationsInfo);
        }

        private static Relation GetRelationInfo(this IResolverContext ctx, Entity entity)
        {
            var dict = (Dictionary<Entity, Relation>) ctx.ScopedContextData.GetValueOrDefault(LastRelation);
            return dict?.GetOrDefault(entity);
        }
    }
}
