using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.Ontology;
using Attribute = IIS.Core.Ontology.Attribute;

namespace IIS.Core.GraphQL.Entities.Resolvers
{
    public interface IOntologyQueryResolver
    {
        ObjectType ResolveAbstractType(IResolverContext context, object resolverResult);
        Task<Guid> ResolveId(IResolverContext ctx);
        Task<Entity> ResolveEntity(IResolverContext ctx, EntityType type);
        Task<Tuple<IEnumerable<EntityType>, NodeFilter>> ResolveEntityList(IResolverContext ctx, EntityType type);
        Task<object> ResolveAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType);
        Task<IEnumerable<Relation>> ResolveMultipleAttributeRelation(IResolverContext ctx, EmbeddingRelationType relationType);
        Task<object> ResolveMultipleAttributeRelationTarget(IResolverContext ctx);
        Task<object> ResolveAttributeValue(IResolverContext ctx, Attribute attribute);
        Task<object> ResolveEntityRelation(IResolverContext ctx, EmbeddingRelationType relationType);
        Task<Relation> ResolveParentRelation(IResolverContext ctx);
        Task<DateTime> ResolveCreatedAt(IResolverContext ctx);
        Task<DateTime> ResolveUpdatedAt(IResolverContext ctx);
        Task<Tuple<IEnumerable<EntityType>, NodeFilter>>  GetAllEntities(IResolverContext ctx);
    }
}
