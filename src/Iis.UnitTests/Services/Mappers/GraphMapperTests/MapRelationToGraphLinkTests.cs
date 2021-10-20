using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.OntologyData.DataTypes;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Ontology.Data;
using Newtonsoft.Json.Linq;
using Iis.Domain.Graph;
using AutoFixture.Xunit2;
using System;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapRelationToGraphLinkTests
    {
        [Fact]
        public void Should_ReturnNull_WhenRelationIsNull()
        {
            IRelation relation = null;

            var result = GraphTypeMapper.MapRelationToGraphLink(relation);

            result.Should().BeNull();
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphLink_WhenTargetNodeIsObjectSign(RelationData relation)
        {
            relation._targetNode = CustomNode.Create(EntityTypeNames.ObjectSign);
            relation._node = CustomNode.Create(EntityTypeNames.ObjectSign);
            var expected = CreateGraphLink(relation, _ => _.TargetNode.NodeType.Title);

            var result = GraphTypeMapper.MapRelationToGraphLink(relation);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoData(EntityTypeNames.Object)]
        [InlineAutoData(EntityTypeNames.ObjectOfStudy)]
        [InlineAutoData(EntityTypeNames.AccessLevel)]
        [InlineAutoData(EntityTypeNames.Enum)]
        [InlineAutoData(EntityTypeNames.Event)]
        [InlineAutoData(EntityTypeNames.FuzzyDate)]
        [InlineAutoData(EntityTypeNames.Photo)]
        [InlineAutoData(EntityTypeNames.Wiki)]
        public void Should_MapToGraphLink_WhenTargetNodeIsNotObjectSign(EntityTypeNames typeName, RelationData relation)
        {
            relation._targetNode = CustomNode.Create(typeName);
            relation._node = CustomNode.Create(typeName);
            var expected = CreateGraphLink(relation, _ => _.Node.NodeType.Title);

            var result = GraphTypeMapper.MapRelationToGraphLink(relation);

            result.Should().BeEquivalentTo(expected);
        }

        private static GraphLink CreateGraphLink(IRelation relation, Func<IRelation, string> nameFunc)
        {
            var extra = new JObject();

            extra.Add(GraphTypeExtraPropNames.Type, relation.RelationTypeName);
            extra.Add(GraphTypeExtraPropNames.Name, nameFunc(relation));

            return new GraphLink
            {
                Id = relation.Id,
                From = relation.SourceNodeId,
                To = relation.TargetNodeId,
                Extra = extra
            };
        }
    }
}