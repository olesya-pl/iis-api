using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using IIS.Core.GraphQL.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.OntologySchema
{
    public class OshGetEntityTypesTests
    {
        private EntityType GetGraphQlEntityType(IOntologySchema schema, string name)
        {
            var collection = (new Query()).GetEntityTypes(schema).Result;
            var items = collection.GetItems();
            return items.FirstOrDefault(nt => nt.Code == name);
        }
        private void AssertAttributes(List<IEntityAttribute> attributes, List<string> names)
        {
            Assert.Equal(names.Count, attributes.Count);
            Assert.Equal(names.OrderBy(s => s), attributes.Select(att => att.Code).OrderBy(s => s));
        }
        [Fact]
        public void AttributesTest()
        {
            var (schema, creator) = OntologyDataCreator.GetBaseTestOntology();
            var objectOfStudy = GetGraphQlEntityType(schema, EntityTypeNames.ObjectOfStudy.ToString());
            var attributes = objectOfStudy.GetAttributes().ToList();
            AssertAttributes(attributes, new List<string> { "affiliation", "importance", "lastConfirmedAt", "sign", "title" });
        }
        [Fact]
        public void AttributesDirectDisabledTest()
        {
            var (schema, creator) = OntologyDataCreator.GetBaseTestOntology();
            var entityType = creator.CreateEntityType("TestEntity");
            creator.CreateAttributeType(entityType.Id, "field1");
            creator.CreateAttributeType(entityType.Id, "field2");
            creator.CreateAttributeType(entityType.Id, "field3", meta: new SchemaMeta { Hidden = true });

            var gqlType = GetGraphQlEntityType(schema, "TestEntity");

            var attributes = gqlType.GetAttributes().ToList();
            AssertAttributes(attributes, new List<string> { "field1", "field2" });
        }
        [Fact]
        public void AttributesDeepDisabledTest()
        {
            var (schema, creator) = OntologyDataCreator.GetBaseTestOntology();
            var objectOfStudy = schema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            
            var entityType = creator.CreateEntityType("TestEntity", ancestorId: objectOfStudy.Id);
            creator.CreateAttributeType(entityType.Id, "lastConfirmedAt", meta: new SchemaMeta { Hidden = true });

            var gqlType = GetGraphQlEntityType(schema, "TestEntity");
            var attributes = gqlType.GetAttributes().ToList();
            AssertAttributes(attributes, new List<string> { "affiliation", "importance", "sign", "title" });
        }
        [Fact]
        public void TargetTypesTest()
        {
            var (schema, creator) = OntologyDataCreator.GetBaseTestOntology();
            var objectOfStudy = schema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());


        }
    }
}
