using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using Iis.OntologySchema.DataTypes;
using System;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class CustomNode : NodeData
    {
        public static NodeData Create(EntityTypeNames typeName)
        {
            var node = new CustomNode();
            node.NodeType = new SchemaNodeType() { Name = typeName.ToString() };

            return node;
        }
    }

    public class CustomMaterialFeature : MaterialFeature
    {
        public new Guid Id { get; set; }
        public new string Relation { get; set; }
        public new string Value { get; set; }

        public static MaterialFeature Create(EntityTypeNames typeName)
        {
            var nodeType = new SchemaNodeType() { Name = typeName.ToString() };
            var node = new Entity(Guid.NewGuid(), nodeType);
            node.OriginalNode = CustomNode.Create(typeName);

            return new CustomMaterialFeature
            {
                Node = node
            };
        }
    }
}