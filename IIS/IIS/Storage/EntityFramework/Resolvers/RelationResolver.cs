using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class RelationResolver : IFieldResolver
    {
        public object Resolve(ResolveFieldContext context)
        {
            var relation = (ORelation)context.Source;
            if (relation.SourceId < 1) return new { Id = 0, relation.Target.CreatedAt, IsInferred = false };
            return new { relation.Id, relation.StartsAt, relation.EndsAt, relation.CreatedAt, relation.IsInferred };
        }
    }
}
