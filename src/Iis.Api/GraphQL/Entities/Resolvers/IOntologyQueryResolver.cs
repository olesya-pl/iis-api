using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Attribute = Iis.Domain.Attribute;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public interface IOntologyQueryResolver
    {
        ObjectType ResolveAbstractType(IResolverContext context, object resolverResult);
        Task<Guid> ResolveId(IResolverContext ctx);
        Task<Entity> ResolveEntity(IResolverContext ctx, INodeTypeModel type);
        Task<Tuple<IEnumerable<INodeTypeModel>, ElasticFilter, IEnumerable<Guid>>> ResolveEntityList(IResolverContext ctx, INodeTypeModel type);
        Task<object> ResolveAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx);
        Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute);
        Task<object> ResolveEntityRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        Task<Relation> ResolveParentRelation(IResolverContext ctx);
        Task<DateTime> ResolveCreatedAt(IResolverContext ctx);
        Task<DateTime> ResolveUpdatedAt(IResolverContext ctx);
        Task<Tuple<IEnumerable<INodeTypeModel>, ElasticFilter, IEnumerable<Guid>>>  GetAllEntities(IResolverContext ctx);
        Task<List<GeoCoordinate>> ResolveCoordinates(IResolverContext ctx);
    }
}
