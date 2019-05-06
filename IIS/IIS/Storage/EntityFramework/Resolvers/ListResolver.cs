using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class ListResolver : IFieldResolver
    {
        private readonly ContourContext _context;

        public ListResolver(ContourContext context)
        {
            _context = context;
        }

        public object Resolve(ResolveFieldContext context)
        {
            var relation = context.Source as ORelation;
            var id = relation.Target.Id;
            var code = context.FieldName;
            var relations = _context.Relations
                .Where(r => r.Type.Code == code && r.Source.Id == id)
                .Include(r => r.Target).ThenInclude(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .ToArray();

            return relations;
        }
    }
}
