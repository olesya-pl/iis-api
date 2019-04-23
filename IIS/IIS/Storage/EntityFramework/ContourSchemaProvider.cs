using System.Threading.Tasks;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework
{
    public class ContourSchemaProvider : ISchemaProvider
    {
        private readonly ContourContext _context;
        private readonly ISchemaGenerator _schemaGenerator;

        public ContourSchemaProvider(string connectionString, ISchemaGenerator schemaGenerator)
        {
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            _context = new ContourContext(options);
            _schemaGenerator = schemaGenerator;
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var entityTypes = await _context.EntityTypes
                .Include(_ => _.EntityTypeAttributes).ThenInclude(_ => _.Attribute)
                .Include(_ => _.BackwardRelationRestrictions)
                .Include(_ => _.ForwardRelationRestrictions)
                .ToArrayAsync();

            var schema = _schemaGenerator.Generate(entityTypes, "Endpoints");
            return schema;
        }
    }
}
