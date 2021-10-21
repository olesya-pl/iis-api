using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Domain.Materials;
using System;
using Iis.Domain.Graph;
using Newtonsoft.Json.Linq;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapMaterialToGraphLinkTests
    {
        [Fact]
        public void Should_ReturnNull_WhenMaterialIsNull()
        {
            Material material = null;

            var result = GraphTypeMapper.MapMaterialToGraphLink(material, Guid.NewGuid());

            result.Should().BeNull();
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphLink(Material material, Guid fromNodeId)
        {
            var expected = CreateGraphLink(material, fromNodeId);

            var result = GraphTypeMapper.MapMaterialToGraphLink(material, fromNodeId);

            result.From.Should().Be(expected.From);
            result.To.Should().Be(expected.To);
            result.Extra.Should().BeEquivalentTo(expected.Extra);
        }

        private static GraphLink CreateGraphLink(Material material, Guid fromNodeId)
        {
            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.Type, GraphTypePropValues.MaterialGraphLinkTypePropValue);
            extraObject.Add(GraphTypeExtraPropNames.Name, GraphTypePropValues.MaterialGraphLinkNamePropValue);

            return new GraphLink
            {
                Id = Guid.NewGuid(),
                From = fromNodeId,
                To = material.Id,
                Extra = extraObject
            };
        }
    }
}