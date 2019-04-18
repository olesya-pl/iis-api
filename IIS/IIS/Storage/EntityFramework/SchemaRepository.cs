using System;
using System.Threading.Tasks;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Storage.EntityFramework
{
    public class SchemaRepository : ISchemaRepository
    {
        private readonly ContourContext _context;

        public SchemaRepository(string connectionString)
        {
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            _context = new ContourContext(options);
        }

        public async Task<object> Ping()
        {
            return await _context.EntityTypes.ToListAsync();
        }
    }
}
