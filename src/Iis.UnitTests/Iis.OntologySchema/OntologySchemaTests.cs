using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.OntologySchema
{
    public class OntologySchemaTests
    {
        [Fact]
        public void CreateEntityTest()
        {
            var schema = Utils.GetEmptyOntologySchema();
            var updateParameter = new NodeTypeUpdateParameter
            {
                Name = "Entity1",
                Title = "Abc"
            };
            schema.UpdateNodeType(updateParameter);
            var entityTypes = schema.GetEntityTypes().ToList();
            Assert.Single(entityTypes);
            var type = entityTypes[0];
            Assert.Equal("Entity1", type.Name);
            Assert.Equal("Abc", type.Title);

            var updateParameter2 = new NodeTypeUpdateParameter
            {
                Id = type.Id,
                Title = "Abc_"
            };
            schema.UpdateNodeType(updateParameter2);
            entityTypes = schema.GetEntityTypes().ToList();
            Assert.Single(entityTypes);
            type = entityTypes[0];
            Assert.Equal("Entity1", type.Name);
            Assert.Equal("Abc_", type.Title);
        }

    }
}
