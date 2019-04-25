using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework.Resolvers
{
    public class EndpointsResolver : IFieldResolver
    {
        private readonly ContourContext _context;

        public EndpointsResolver(ContourContext context)
        {
            _context = context;
        }

        public object Resolve(ResolveFieldContext context)
        {
            var code = context.FieldName.Camelize();
            var data = _context.EntityTypes.Where(e => e.Code == code || e.Parent.Code == code)
                .SelectMany(e => e.Entities)
                .Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .ToArray();

            return data;
        }
    }
}
