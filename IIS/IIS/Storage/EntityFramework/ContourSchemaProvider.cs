using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using IIS.Storage.EntityFramework.Resolvers;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework
{
    public class ContourSchemaProvider : ISchemaProvider
    {
        private readonly ContourContext _context;

        public ContourSchemaProvider(ContourContext context)
        {
            _context = context;
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var entityTypes = await _context.EntityTypes
                .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
                .Include(_ => _.BackwardRestrictions)
                .Include(_ => _.ForwardRestrictions)
                .ToArrayAsync();

            var schema = Generate(entityTypes, "Endpoints");
            return schema;
        }

        public ISchema Generate(IEnumerable<OType> entityTypes, string rootName)
        {
            var schema = new Schema { Query = new ObjectGraphType { Name = rootName } };
            var types = entityTypes.Where(e => e.Type != "relation")
                .Select(e => new GraphTypeBuilder(_context, e).Build()).ToArray();

            foreach (var type in types)
                schema.Query.AddField(new FieldType
                {
                    Name = type.Name,
                    ResolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(type))),
                    Resolver = new EndpointsResolver(_context)
                });

            return schema;
        }
    }
}
