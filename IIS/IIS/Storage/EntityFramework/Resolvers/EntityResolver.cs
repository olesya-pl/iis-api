using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class EntityResolver : IFieldResolver
    {
        private readonly ContourContext _context;

        public EntityResolver(ContourContext context)
        {
            _context = context;
        }

        public object Resolve(ResolveFieldContext context)
        {
            var code = context.FieldName;
            var entity = context.Source as OEntity;
            var id = entity.Id;
            var data = _context.Relations
                .Where(r => r.Type.Code == code && r.Source.Id == id)
                .Include(r => r.Target).ThenInclude(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .SingleOrDefault();

            return data.Target;
        }
    }
}
