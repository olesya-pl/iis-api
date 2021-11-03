using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Domain.Materials;
using System;
using Iis.Domain.Graph;
using Newtonsoft.Json.Linq;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapRelatedMaterialGraphLinkTests
    {
        [Fact]
        public void Should_ReturnNull_WhenMaterialIsNull()
        {
            Material material = null;

            var result = GraphTypeMapper.MapRelatedMaterialGraphLink(material, Guid.NewGuid());

            result.Should().BeNull();
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphLink(Material material, Guid fromNodeId)
        {
            var expected = CreateGraphLink(material, fromNodeId);

            var result = GraphTypeMapper.MapRelatedMaterialGraphLink(material, fromNodeId);

            result.Should().BeEquivalentTo(expected, opt => opt.Excluding(_ => _.Id));
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