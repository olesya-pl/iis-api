using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.DataLoaders;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Entities.ObjectTypes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.Interfaces.Elastic;
using Attribute = Iis.Domain.Attribute;
using Node = Iis.Domain.Node;
using Relation = Iis.Domain.Relation;
using Iis.Interfaces.Constants;
using Iis.Domain.Users;
using Iis.Interfaces.SecurityLevels;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public class OntologyQueryResolver : IOntologyQueryResolver
    {
        private const string LastRelation = "LastRelation";
        private const string ObjectNotFound = "Об'єкт не знайдено";

        public ObjectType ResolveAbstractType(IResolverContext context, object resolverResult)
        {
            var node = (Node) resolverResult;
            var typeName = OntologyObjectType.GetName(node.Type);
            return context.Schema.GetType<ObjectType>(typeName);
        }

        // ----- Root Entity resolvers ----- //

        public Task<Guid> ResolveId(IResolverContext ctx)
        {
            return Task.FromResult(ctx.Parent<Node>().Id);
        }

        public async Task<Entity> ResolveEntity(IResolverContext ctx, INodeTypeLinked type)
        {
            var id = ctx.Argument<Guid>("id");
            var userService = ctx.Service<IUserService>();
            var tokenPayload = ctx.GetToken();

            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create<Guid, INodeTypeLinked>(id, null), default);
            var securityLevelChecker = ctx.Service<ISecurityLevelChecker>();

            if (!securityLevelChecker.AccessGranted(tokenPayload.User.SecurityLevelsIndexes, node.OriginalNode.GetSecurityLevelIndexes()))
                throw new Exception($"{FrontEndErrorCodes.NotFound}:{ObjectNotFound}");

            if (!userService.IsAccessLevelAllowedForUser(tokenPayload.User.AccessLevel, node.OriginalNode.GetAccessLevelIndex()))
                throw new Exception($"{FrontEndErrorCodes.NotFound}:{ObjectNotFound}");

            return node as Entity; // return null if node was not entity
        }

        public Task<Tuple<IEnumerable<INodeTypeLinked>, ElasticFilter, IEnumerable<Guid>, User>> ResolveEntityList(IResolverContext ctx, INodeTypeLinked type)
        {
            var nf = ctx.CreateNodeFilter(type);

            var filter = ctx.Argument<FilterInput>("filter");

            var tokenPayload = ctx.GetToken();

            IEnumerable<Guid> ids = Array.Empty<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }

            var result = Tuple.Create((IEnumerable<INodeTypeLinked>) new[] {type}, nf, ids, tokenPayload.User);

            return Task.FromResult(result);
        }

        // ----- Relations to attributes ----- //

        // Resolve single entity-attribute relation. Return any scalar type.
        public async Task<object> ResolveAttributeRelation(IResolverContext ctx, INodeTypeLinked relationType)
        {
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            if (relationType.IsComputed)
            {

                var formula = relationType.MetaObject?.Formula;
                return parent.OriginalNode.ResolveFormula(formula);
            }
            var relation = node?.GetRelationOrDefault(relationType);
            if (relation == null) return null;
            return await ResolveAttributeValue(ctx, relation.AttributeTarget);
        }

        // resolve multiple entity-[attribute] relation
        public async Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, INodeTypeLinked relationType)
        {
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            return node.GetRelations(relationType.Name);
        }

        // Parent - embedding relation to attribute, resolve attribute value here
        public async Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx)
        {
            var parent = ctx.Parent<Relation>();
            return await ResolveAttributeValue(ctx, parent.AttributeTarget);
        }

        // Return any scalar type for given attribute
        public Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute)
        {
            return Task.FromResult(attribute.Value);
        }

        // ----- Relations to entities ----- //

        // Resolve one or multiple relations to entity. Return either Entity or IEnumerable<Entity>
        public async Task<object> ResolveEntityRelation(IResolverContext ctx, INodeTypeLinked relationType)
        {
            var ontologyService = ctx.Service<IOntologyService>();
            var parent = ctx.Parent<Node>();
            var node = await ctx.DataLoader<NodeDataLoader>().LoadAsync(Tuple.Create(parent.Id, relationType), default);
            var relations = node.GetRelations(relationType.Name);
            if (!relations.GroupBy(r => r.EntityTarget).All(g => g.Count() == 1))
                return relations.Select(r => r.EntityTarget); // Non-unique targets breaks our _relation !!!
            var relationsInfo = relations.ToDictionary(r => r.EntityTarget);
            SetRelationInfo(ctx, relationsInfo); // pass info to _relation
            if (relationType.IsMultiple)
                return relationsInfo.Keys;
            return relationsInfo.Keys.SingleOrDefault();
        }

        // resolver for "_relation" field on schema, passed through context
        public Task<Relation> ResolveParentRelation(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return Task.FromResult(GetRelationInfo(ctx, parent));
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

        public Task<DateTime> ResolveCreatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return Task.FromResult(parent.CreatedAt);
        }

        public Task<DateTime> ResolveUpdatedAt(IResolverContext ctx)
        {
            var parent = ctx.Parent<Entity>();
            return Task.FromResult(parent.UpdatedAt);
        }

        // ------ All entities ----- //

        public Task<Tuple<IEnumerable<INodeTypeLinked>, ElasticFilter, IEnumerable<Guid>, User>> GetAllEntities(IResolverContext ctx)
        {
            var filter = ctx.Argument<AllEntitiesFilterInput>("filter");
            var schema = ctx.Service<IOntologySchema>();

            var types = schema.GetEntityTypes();
            if (filter?.Types != null)
                types = types.Where(et => filter.Types.Contains(et.Name)).ToList();

            IEnumerable<Guid> ids = new List<Guid>();

            if (filter != null && filter.MatchList?.Any() == true)
            {
                ids = filter.MatchList;
            }
            var tokenPayload = ctx.GetToken();
            return Task.FromResult(Tuple.Create(types, ctx.CreateNodeFilter(), ids, tokenPayload.User));
        }
        public async Task<List<GeoCoordinate>> ResolveCoordinatesAsync(IResolverContext ctx)
        {
            var result = new List<GeoCoordinate>();
            var parentNode = ctx.Parent<Node>();
            var attributeNodes = parentNode.OriginalNode.GetAllAttributeNodes(Iis.Interfaces.Ontology.Schema.ScalarType.Geo);
            var geoCoordinates = attributeNodes.Select(n => n.Attribute.ValueAsGeoCoordinates);

            foreach (var attributeNode in attributeNodes)
            {
                var relation = attributeNode.IncomingRelations.Single();

                if (relation.SourceNode.NodeType.IsObjectSign)
                    continue;

                var geoCoordinate = attributeNode.Attribute.ValueAsGeoCoordinates;
                var label = relation.SourceNodeId == parentNode.Id ?
                    relation.Node.NodeType.Title :
                    relation.SourceNode.IncomingRelations.Single().Node.NodeType.Title;

                result.Add(new GeoCoordinate
                {
                    Label = label,
                    Lat = geoCoordinate.Latitude,
                    Long = geoCoordinate.Longitude,
                    PropertyName = attributeNode.GetDotName()
                });
            }

            if(parentNode.OriginalNode.NodeType.IsObjectSign)
            {
                var locationService = ctx.Service<ILocationHistoryService>();

                var latestCoordinate = await locationService.GetLatestLocationHistoryAsync(parentNode.OriginalNode.Id);

                if(latestCoordinate is null) return result;

                var attributeNode = parentNode.OriginalNode.GetSingleProperty("value");
                var nodeValue = attributeNode?.Value;

                var propertyValue = string.IsNullOrWhiteSpace(nodeValue)
                    ? string.Empty
                    : $"[{nodeValue}]";

                var label = $"{parentNode.OriginalNode.NodeType.Title}{propertyValue}";

                result.Add(new GeoCoordinate{
                    Label = label,
                    Lat = latestCoordinate.Lat,
                    Long = latestCoordinate.Long,
                    PropertyName = "sign.location"
                });
            }

            return result;
        }

        public async Task<string> ResolveCreatedBy(IResolverContext ctx)
        {
            var node = ctx.Parent<Node>();
            var createdBy = node.GetAttributeValue("createdBy");

            if (createdBy == null)
            {
                return string.Empty;
            }

            if (Guid.TryParse(createdBy.ToString(), out var createdById))
            {
                var userService = ctx.Service<IUserService>();
                var createdByUser = await userService.GetUserAsync(createdById);
                if (createdByUser == null)
                {
                    return string.Empty;
                }
                return $"{createdByUser.LastName} {createdByUser.FirstName}";
            }
            return string.Empty;
        }

        public string ResolveIconName(IResolverContext ctx) =>
            ctx.Parent<Node>().OriginalNode.NodeType.GetIconName();

        public string ResolveTitle(IResolverContext ctx) =>
            ctx.Parent<Node>().OriginalNode.GetTitleValue();

        public int ResolveAccessLevel(IResolverContext ctx) =>
            ctx.Parent<Node>().OriginalNode.GetAccessLevelIndex();
    }
}
