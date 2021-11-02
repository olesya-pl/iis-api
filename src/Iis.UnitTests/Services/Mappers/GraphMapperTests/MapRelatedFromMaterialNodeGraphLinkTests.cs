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
    public class MapRelatedFromMaterialNodeGraphLinkTests
    {
        private const string SignPropertyName = "sign";
        private const string SatellitePhonePropertyTitle = "Супутниковий телефон";

        [Theory, RecursiveAutoData]
        public void Should_ReturnNull_WhenMaterialIsNull(CustomNode node)
        {
            Material material = null;

            var result = GraphTypeMapper.MapRelatedFromMaterialNodeGraphLink(material, node);

            result.Should().BeNull();
        }

        [Theory, RecursiveAutoData]
        public void Should_MapToNodeGraphLinkWhenNodeIsNotObjectSign(Material material)
        {
            var node = CustomNode.Create(EntityTypeNames.Object, EntityTypeNames.Object.ToString());
            var expected = CreateGraphLink(material, node, $"related{node.NodeType.Name}", node.NodeType.Title);

            var result = GraphTypeMapper.MapRelatedFromMaterialNodeGraphLink(material, node);

            result.Should().BeEquivalentTo(expected, opt => opt.Excluding(_ => _.Id));
        }

        [Theory, RecursiveAutoData]
        public void Should_MapToNodeGraphLinkWhenNodeIsObjectSign(Material material)
        {
            var node = CustomNode.Create(EntityTypeNames.ObjectSign, SatellitePhonePropertyTitle);
            var expected = CreateGraphLink(material, node, SignPropertyName, SatellitePhonePropertyTitle);

            var result = GraphTypeMapper.MapRelatedFromMaterialNodeGraphLink(material, node);

            result.Should().BeEquivalentTo(expected, opt => opt.Excluding(_ => _.Id));
        }

        private static GraphLink CreateGraphLink(Material material, INode node, string expectedExtraType, string expectedExtraName)
        {
            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.Type, expectedExtraType);
            extraObject.Add(GraphTypeExtraPropNames.Name, expectedExtraName);

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