using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using IIS.Core.GraphQL.DataLoaders;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Attribute = Iis.Domain.Attribute;
using Node = Iis.Domain.Node;
using Relation = Iis.Domain.Relation;

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

        public async Task<Entity> ResolveEntity(IResolverContext ctx, IEntityTypeModel type)
        {
            var id = ctx.Argument<Guid>("id");
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create<Guid, IEmbeddingRelationTypeModel>(id, null), default);
            return node as Entity; // return null if node was not entity
        }

        public async Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>> ResolveEntityList(IResolverContext ctx, IEntityTypeModel type)
        {
            var nf = ctx.CreateNodeFilter(type);

            var filter = ctx.Argument<FilterInput>("filter");

            IEnumerable<Guid> ids = new List<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }

            return Tuple.Create((IEnumerable<IEntityTypeModel>) new[] {type}, nf, ids);
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public async Task<object> ResolveAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
        {
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            if (relationType.IsComputed())
            {

                var formula = (relationType.Meta as IAttributeRelationMeta)?.Formula;
                return parent.OriginalNode.ResolveFormula(formula);
            }
            var relation = node?.GetRelationOrDefault(relationType);
            if (relation == null) return null;
            return await ResolveAttributeValue(ctx, relation.AttributeTarget);
        }

        // resolve multiple entity-[attribute] relation
        public async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
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
        public async Task<object> ResolveEntityRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType)
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

        public Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>> GetAllEntities(IResolverContext ctx)
        {
            var filter = ctx.Argument<AllEntitiesFilterInput>("filter");
            var ontology = ctx.Service<IOntologyModel>();

            var types = ontology.EntityTypes;
            if (filter?.Types != null)
                types = types.Where(et => filter.Types.Contains(et.Name)).ToList();

            IEnumerable<Guid> ids = new List<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }

            return Task.FromResult(Tuple.Create(types, ctx.CreateNodeFilter(), ids));
        }

        public async Task<List<GeoCoordinate>> ResolveCoordinates(IResolverContext ctx)
        {
            var parentNode = ctx.Parent<Node>();
            var attriburteNodes = parentNode.OriginalNode.GetAllAttributeNodes(Iis.Interfaces.Ontology.Schema.ScalarType.Geo);
            var geoCoordinates = attriburteNodes.Select(n => n.Attribute.ValueAsGeoCoordinates);

            return geoCoordinates.Select(x => new GeoCoordinate
            {
                Label = parentNode.OriginalNode.NodeType.Title,
                Lat = x.Latitude,
                Long = x.Longitude
            }).ToList();
        }
    }
}
