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
            var source = context.Source as OEntity;
            var id = source.Id;
            var code = context.FieldName;
            var data = _context.Relations
                .Where(r => r.Type.Code == code && r.Source.Id == id)
                .Select(r => r.Target)//todo: _relation?
                .Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .ToArray();

            return data;
        }
    }
}
