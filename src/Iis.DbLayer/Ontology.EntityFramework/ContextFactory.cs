using Iis.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Ontology.EntityFramework
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
            var options = new DbContextOptionsBuilder<OntologyContext>()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;

            return new OntologyContext(options);
        }
    }
}
