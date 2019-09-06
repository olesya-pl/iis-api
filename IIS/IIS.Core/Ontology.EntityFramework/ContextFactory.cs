using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ContextFactory
    {
        private readonly string _connectionString;

        public ContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public OntologyContext CreateContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;

            return new OntologyContext(options);
        }
    }
}
