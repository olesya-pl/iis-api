using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class IsEligibleForGraphByNodeTypeTests
    {
        [Fact]
        public void Should_BeTrue_WhenNodeIsObject()
        {
            var node = CustomNode.Create(EntityTypeNames.Object);

            var result = GraphTypeMapper.IsEligibleForGraphByNodeType(node);

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_BeTrue_WhenNodeIsObjectSign()
        {
            var node = CustomNode.Create(EntityTypeNames.ObjectSign);

            var result = GraphTypeMapper.IsEligibleForGraphByNodeType(node);

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(EntityTypeNames.ObjectOfStudy)]
        [InlineData(EntityTypeNames.AccessLevel)]
        [InlineData(EntityTypeNames.Enum)]
        [InlineData(EntityTypeNames.Event)]
        [InlineData(EntityTypeNames.FuzzyDate)]
        [InlineData(EntityTypeNames.Photo)]
        [InlineData(EntityTypeNames.Wiki)]
        public void Should_BeFalse_WhenOtherTypes(EntityTypeNames typeName)
        {
            var node = CustomNode.Create(typeName);

            var result = GraphTypeMapper.IsEligibleForGraphByNodeType(node);

            result.Should().BeFalse();
        }
    }
}