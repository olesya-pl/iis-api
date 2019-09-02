using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using Attribute = IIS.Core.Ontology.Attribute;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class OntologyQueryResolver : IOntologyQueryResolver
    {
        private const string LastRelation = "LastRelation";

        public ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node) resolverResult;
            var typeName = OntologyObjectType.GetName(node.Type);
            return context.Schema.GetType<ObjectType>(typeName);
        }

        // ----- Root Entity resolvers ----- //

        public async Task<Guid> ResolveId(IResolverContext ctx)
        {
            return ctx.Parent<Node>().Id;
        }

        public async Task<Entity> ResolveEntity(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var id = ctx.Argument<Guid>("id");
            var node = await ontologyService.LoadNodesAsync(id, null);
            return node as Entity; // return null if node was not entity
        }

        public async Task<IEnumerable<Entity>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var list = await ontologyService.GetNodesByTypeAsync(type); // Direct type
            if (pagination != null)
                list = list.Skip(pagination.Page * pagination.PageSize).Take(pagination.PageSize);
            return list.OfType<Entity>();
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            var relation = node.GetRelation(relationType);
            if (relation == null) return null;
            return await ResolveAttributeValue(ctx, relation.AttributeTarget);
        }

        // resolve multiple entity-[attribute] relation
        public async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var ontologyService = ctx.Service<IOntologyService>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            return node.GetRelations(relationType);
        }

        // Parent - embedding relation to attribute, resolve attribute value here
        public async Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx)
        {
            var parent = ctx.Parent<Relation>();
            return await ResolveAttributeValue(ctx, parent.AttributeTarget);
        }

        // Return any scalar type for given attribute
        public async Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute)
        {
            return attribute.Value;
        }

        // ----- Relations to entities ----- //

        // Resolve one or multiple relations to entity. Return either Entity or IEnumerable<Entity>
        public async Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ontologyService.LoadNodesAsync(parent.Id, new[] {relationType});
            var relations = node.GetRelations(relationType);
            var relationsInfo = relations.ToDictionary(r => r.EntityTarget);
            SetRelationInfo(ctx, relationsInfo); // pass info to _relation
            if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                return relationsInfo.Keys;
            return relationsInfo.Keys.SingleOrDefault();
        }

        // resolver for "_relation" field on schema, passed through context
        public async Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return GetRelationInfo(ctx, parent);
        }

        // ----- Context operations ----- //

        private static void SetRelationInfo(IResolverContext ctx, Dictionary<Entity, Relation> relationsInfo)
        {
            ctx.ScopedContextData = ctx.ScopedContextData.SetItem(LastRelation, relationsInfo);
        }

        private static Relation GetRelationInfo(IResolverContext ctx, Entity entity)
        {
            var dict = (Dictionary<Entity, Relation>) ctx.ScopedContextData.GetValueOrDefault(LastRelation);
            return dict?.GetOrDefault(entity);
        }
    }
}
