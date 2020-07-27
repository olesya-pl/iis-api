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
        [Fact]
        public void GetAttributeInfoTest_NoInheritance()
        {
            var schema = Utils.GetEmptyOntologySchema();
            CreateEntities_Aliases(schema);
            var attributeInfos = schema.GetAttributesInfo("GrandParentEntity");
            Assert.Equal(3, attributeInfos.Items.Count);
            var items = attributeInfos.Items.OrderBy(ai => ai.DotName).ToList();
            var item = items[0];
            Assert.Equal("Column1", item.DotName);
            Assert.Equal(ScalarType.String, item.ScalarType);
            Assert.Equal(new string[] { "Col1GrandAlias1", "Col1GrandAlias2" }, item.AliasesList);
            item = items[1];
            Assert.Equal("Column2", item.DotName);
            Assert.Equal(ScalarType.Decimal, item.ScalarType);
            Assert.Equal(new string[] { "Col2GrandAlias1", "Col2GrandAlias2" }, item.AliasesList);
            item = items[2];
            Assert.Equal("Column3", item.DotName);
            Assert.Equal(ScalarType.Date, item.ScalarType);
            Assert.Equal(new string[] { "Col3GrandAlias1", "Col3GrandAlias2" }, item.AliasesList);
        }
        [Fact]
        public void GetAttributeInfoTest_InheritanceDeep1()
        {
            var schema = Utils.GetEmptyOntologySchema();
            CreateEntities_Aliases(schema);
            var attributeInfos = schema.GetAttributesInfo("ParentEntity");
            Assert.Equal(3, attributeInfos.Items.Count);
            var items = attributeInfos.Items.OrderBy(ai => ai.DotName).ToList();
            var item = items[0];
            Assert.Equal("Column1", item.DotName);
            Assert.Equal(ScalarType.String, item.ScalarType);
            Assert.Equal(new string[] { "Col1GrandAlias1", "Col1GrandAlias2" }, item.AliasesList);
            item = items[1];
            Assert.Equal("Column2", item.DotName);
            Assert.Equal(ScalarType.Decimal, item.ScalarType);
            Assert.Equal(new string[] { "Col2ParentAlias1", "Col2ParentAlias2" }, item.AliasesList);
            item = items[2];
            Assert.Equal("Column3", item.DotName);
            Assert.Equal(ScalarType.Date, item.ScalarType);
            Assert.Equal(new string[] { "Col3ParentAlias1", "Col3ParentAlias2" }, item.AliasesList);
        }
        [Fact]
        public void GetAttributeInfoTest_InheritanceDeep2()
        {
            var schema = Utils.GetEmptyOntologySchema();
            CreateEntities_Aliases(schema);
            var attributeInfos = schema.GetAttributesInfo("Entity");
            Assert.Equal(3, attributeInfos.Items.Count);
            var items = attributeInfos.Items.OrderBy(ai => ai.DotName).ToList();
            var item = items[0];
            Assert.Equal("Column1", item.DotName);
            Assert.Equal(ScalarType.String, item.ScalarType);
            Assert.Equal(new string[] { "Col1GrandAlias1", "Col1GrandAlias2" }, item.AliasesList);
            item = items[1];
            Assert.Equal("Column2", item.DotName);
            Assert.Equal(ScalarType.Decimal, item.ScalarType);
            Assert.Equal(new string[] { "Col2ParentAlias1", "Col2ParentAlias2" }, item.AliasesList);
            item = items[2];
            Assert.Equal("Column3", item.DotName);
            Assert.Equal(ScalarType.Date, item.ScalarType);
            Assert.Equal(new string[] { "Col3Alias1", "Col3Alias2" }, item.AliasesList);
        }

        private void CreateEntities_Aliases(IOntologySchema schema)
        {
            var updateParameter = new NodeTypeUpdateParameter
            {
                Name = "GrandParentEntity",
                Aliases = new List<string>
                {
                    "Column1:Col1GrandAlias1,Col1GrandAlias2",
                    "Column2:Col2GrandAlias1,Col2GrandAlias2",
                    "Column3:Col3GrandAlias1,Col3GrandAlias2"
                }
            };
            var grandParentEntity = schema.UpdateNodeType(updateParameter);
            updateParameter = new NodeTypeUpdateParameter
            {
                Name = "Column1",
                EmbeddingOptions = EmbeddingOptions.Optional,
                ScalarType = ScalarType.String,
                ParentTypeId = grandParentEntity.Id
            };
            schema.UpdateNodeType(updateParameter);
            updateParameter = new NodeTypeUpdateParameter
            {
                Name = "Column2",
                EmbeddingOptions = EmbeddingOptions.Optional,
                ScalarType = ScalarType.Decimal,
                ParentTypeId = grandParentEntity.Id
            };
            schema.UpdateNodeType(updateParameter);
            updateParameter = new NodeTypeUpdateParameter
            {
                Name = "Column3",
                EmbeddingOptions = EmbeddingOptions.Optional,
                ScalarType = ScalarType.Date,
                ParentTypeId = grandParentEntity.Id
            };
            schema.UpdateNodeType(updateParameter);
            updateParameter = new NodeTypeUpdateParameter
            {
                Name = "ParentEntity",
                Aliases = new List<string>
                {
                    "Column2:Col2ParentAlias1,Col2ParentAlias2",
                    "Column3:Col3ParentAlias1,Col3ParentAlias2"
                }
            };
            var parentEntity = schema.UpdateNodeType(updateParameter);
            schema.SetInheritance(parentEntity.Id, grandParentEntity.Id);
            updateParameter = new NodeTypeUpdateParameter
            {
                Name = "Entity",
                Aliases = new List<string>
                {
                    "Column3:Col3Alias1,Col3Alias2"
                }
            };
            var entity = schema.UpdateNodeType(updateParameter);
            schema.SetInheritance(entity.Id, parentEntity.Id);
            schema.PutInOrder();
        }
    }
}
