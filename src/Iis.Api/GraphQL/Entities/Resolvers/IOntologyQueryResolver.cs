using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
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
        Task<Entity> ResolveEntity(IResolverContext ctx, INodeTypeLinked type);
        Task<Tuple<IEnumerable<INodeTypeLinked>, ElasticFilter, IEnumerable<Guid>>> ResolveEntityList(IResolverContext ctx, INodeTypeLinked type);
        Task<object> ResolveAttributeRelation(IResolverContext ctx, INodeTypeLinked relationType);
        Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, INodeTypeLinked relationType);
        Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx);
        Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute);
        Task<object> ResolveEntityRelation(IResolverContext ctx, INodeTypeLinked relationType);
        Task<Relation> ResolveParentRelation(IResolverContext ctx);
        Task<DateTime> ResolveCreatedAt(IResolverContext ctx);
        Task<DateTime> ResolveUpdatedAt(IResolverContext ctx);
        Task<Tuple<IEnumerable<INodeTypeLinked>, ElasticFilter, IEnumerable<Guid>>>  GetAllEntities(IResolverContext ctx);
        Task<List<GeoCoordinate>> ResolveCoordinates(IResolverContext ctx);
        Task<string> ResolveCreatedBy(IResolverContext ctx);
        string ResolveIconName(IResolverContext ctx);
    }
}
