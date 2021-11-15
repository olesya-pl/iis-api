using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class IsEligibleForGraphByNodeTypeTests
    {
        [Theory]
        [InlineData(EntityTypeNames.ObjectOfStudy, false)]
        [InlineData(EntityTypeNames.FuzzyDate, false)]
        [InlineData(EntityTypeNames.ObjectSign, true)]
        [InlineData(EntityTypeNames.Event, false)]
        [InlineData(EntityTypeNames.Enum, false)]
        [InlineData(EntityTypeNames.Wiki, false)]
        [InlineData(EntityTypeNames.Object, true)]
        [InlineData(EntityTypeNames.AccessLevel, false)]
        [InlineData(EntityTypeNames.Photo, false)]
        public void CheckEligibilityDependingOnEntityTypeName(EntityTypeNames typeName, bool expectedState)
        {
            var node = CustomNode.Create(typeName);

            var result = GraphTypeMapper.IsEligibleForGraphByNodeType(node);

            result.Should().Be(expectedState);
        }
    }
}