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
            var types = await _context.EntityTypes
                .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
                .Include(_ => _.BackwardRestrictions)//?
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Target)
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.RelationType)
                .ToArrayAsync();

            var schema = Generate(types, "Entities");
            return schema;
        }

        public ISchema Generate(IEnumerable<OTypeEntity> entityTypes, string rootName)
        {
            var schema = new Schema { Query = new ObjectGraphType { Name = rootName } };
            var types = entityTypes.Select(e => new GraphTypeBuilder(_context, e).Build()).ToArray();

            foreach (var type in types)
            {
                schema.Query.AddField(new FieldType
                {
                    Name = type.Name,
                    ResolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(type))),
                    Resolver = new EndpointsResolver(_context),
                    Arguments = new QueryArguments(new QueryArgument(type) { Name = "Id", ResolvedType = new IntGraphType() })
                });
            }
            return schema;
        }
    }
}
