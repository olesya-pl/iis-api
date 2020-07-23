using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Domain;
using Newtonsoft.Json.Linq;
using Relation = IIS.Core.GraphQL.Entities.ObjectTypes.Relation;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public interface IOntologyQueryResolver
    {
        ObjectType ResolveAbstractType(IResolverContext context, object resolverResult);
        Task<Guid> ResolveId(IResolverContext ctx);
        Task<Guid> ResolveMultipleId(IResolverContext ctx);
        Task<JObject> ResolveEntity(IResolverContext ctx, IEntityTypeModel type);
        Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>> ResolveEntityList(IResolverContext ctx, IEntityTypeModel type);
        object ResolveAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        object ResolveMultipleAttributeRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        object ResolveMultipleAttributeRelationTarget(IResolverContext ctx);
        object ResolveEntityRelation(IResolverContext ctx, IEmbeddingRelationTypeModel relationType);
        Task<Relation> ResolveParentRelation(IResolverContext ctx);
        Task<DateTime> ResolveCreatedAt(IResolverContext ctx);
        Task<DateTime> ResolveUpdatedAt(IResolverContext ctx);
        Task<Tuple<IEnumerable<IEntityTypeModel>, ElasticFilter, IEnumerable<Guid>>>  GetAllEntities(IResolverContext ctx);
    }
}
