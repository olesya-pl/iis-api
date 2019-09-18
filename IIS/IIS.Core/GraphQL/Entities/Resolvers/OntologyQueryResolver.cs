using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flee.PublicTypes;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.DataLoaders;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.ComputedProperties;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Meta;
using Microsoft.EntityFrameworkCore;
using Attribute = IIS.Core.Ontology.Attribute;
using EmbeddingOptions = IIS.Core.Ontology.EmbeddingOptions;
using Node = IIS.Core.Ontology.Node;
using Relation = IIS.Core.Ontology.Relation;

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
            var id = ctx.Argument<Guid>("id");
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create<Guid, EmbeddingRelationType>(id, null), default);
            return node as Entity; // return null if node was not entity
        }

        public async Task<Tuple<IEnumerable<EntityType>, NodeFilter>> ResolveEntityList(IResolverContext ctx, EntityType type)
        {
            var nf = ctx.CreateNodeFilter(type);
            return Tuple.Create((IEnumerable<EntityType>) new[] {type}, nf);
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public async Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            if (relationType.IsComputed())
            {
                var computedResolver = ctx.Service<IComputedPropertyResolver>();
                var dependencies = computedResolver.GetRequiredFields(relationType).Select(s => parent.Type.GetProperty(s));
                var result = await ctx.DataLoader<MultipleNodeDataLoader>()
                    .LoadAsync(Tuple.Create(parent.Id, dependencies), default);
                return computedResolver.Resolve(relationType, result);
            }
            var relation = node.GetRelation(relationType);
            if (relation == null) return null;
            return await ResolveAttributeValue(ctx, relation.AttributeTarget);
        }

        // resolve multiple entity-[attribute] relation
        public async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType)
        {
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
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
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            var relations = node.GetRelations(relationType);
            if (!relations.GroupBy(r => r.EntityTarget).All(g => g.Count() == 1))
                return relations.Select(r => r.EntityTarget); // Non-unique targets breaks our _relation !!!
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

        // ----- Created-updated ----- //

        public async Task<DateTime> ResolveCreatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return parent.CreatedAt;
        }

        public async Task<DateTime> ResolveUpdatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return parent.UpdatedAt;
        }

        // ------ All entities ----- //

        public async Task<Tuple<IEnumerable<EntityType>, NodeFilter>> GetAllEntities(IResolverContext ctx)
        {
            var filter = ctx.Argument<AllEntitiesFilterInput>("filter");
            var ontologyService = ctx.Service<IOntologyService>();
            var ontologyProvider = ctx.Service<IOntologyProvider>();

            var ontology = await ontologyProvider.GetOntologyAsync();
            var types = ontology.EntityTypes;
            if (filter?.Types != null)
                types = types.Where(et => filter.Types.Contains(et.Name)).ToList();

            return Tuple.Create(types, ctx.CreateNodeFilter());
        }
    }
}
