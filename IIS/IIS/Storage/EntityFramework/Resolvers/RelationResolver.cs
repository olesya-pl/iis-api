using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class RelationResolver : IFieldResolver
    {
        public object Resolve(ResolveFieldContext context)
        {
            var source = (OEntity)context.Source;
            var code = context.FieldName;
            var relation = source.BackwardRelations.First(r => r.Target.Id == source.Id);
            return new { relation.Id, relation.StartsAt, relation.EndsAt, relation.CreatedAt, relation.IsInferred };
        }
    }
}
