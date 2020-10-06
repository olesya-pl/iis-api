using Iis.DataModel;
using Iis.DbLayer.OntologySchema;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyModelWrapper;
using Iis.OntologySchema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iis.UnitTests.OntologyService
{
    public class RefactorOntologyServiceTests
    {
        [Fact]
        public void Test()
        {
            
        }
        private IOntologyService GetOldService(string connectionString)
        {
            using var context = OntologyContext.GetContext(connectionString);
            var source = new OntologySchemaSource { Data = connectionString, SourceKind = SchemaSourceKind.Database };
            var ontologySchemaService = new OntologySchemaService();
            var schema = ontologySchemaService.LoadFromDatabase(source);
            var ontologyModel = new OntologyWrapper(schema);
            return null;
            
        }
    }
}
