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
            var id = context.Arguments.ContainsKey("id") ? (int)context.Arguments["id"] : 0;
            var code = context.FieldName.Camelize();
            var data = _context.EntityTypes
                .Where(e => e.Code == code || e.Parent.Code == code)
                .SelectMany(e => e.Entities)
                .Where(e => id == 0 || e.Id == id)
                .Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .ToArray()
                .Select(e => new ORelation { Target = e }).ToArray();

            return data;
        }
    }
}
