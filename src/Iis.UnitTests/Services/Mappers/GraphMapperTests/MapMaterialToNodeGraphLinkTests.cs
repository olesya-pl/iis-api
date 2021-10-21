using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Domain.Materials;
using System;
using Iis.Domain.Graph;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapMaterialToNodeGraphLinkTests
    {
        [Theory]
        [RecursiveAutoData]
        public void Should_ReturnNull_WhenMaterialIsNull(CustomNode node)
        {
            Material material = null;

            var result = GraphTypeMapper.MapMaterialToNodeGraphLink(material, node);

            result.Should().BeNull();
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToNodeGraphLink(Material material)
        {
            var node = CustomNode.Create(EntityTypeNames.AccessLevel);
            var expected = CreateGraphLink(material, node);

            var result = GraphTypeMapper.MapMaterialToNodeGraphLink(material, node);

            result.From.Should().Be(expected.From);
            result.To.Should().Be(expected.To);
            result.Extra.Should().BeEquivalentTo(expected.Extra);
        }

        private static GraphLink CreateGraphLink(Material material, INode node)
        {
            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.Type, node.NodeType.Name);
            extraObject.Add(GraphTypeExtraPropNames.Name, node.NodeType.Title);

            return new GraphLink
            {
                Id = Guid.NewGuid(),
                From = material.Id,
                To = node.Id,
                Extra = extraObject
            };
        }
    }
}