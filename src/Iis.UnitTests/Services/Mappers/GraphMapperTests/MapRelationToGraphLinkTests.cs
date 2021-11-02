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
        private const string SignPropertyName = "sign";
        private const string SignPropertyTitle = "Ознаки";
        private const string SatellitePhonePropertyTitle = "Супутниковий телефон";

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
            relation._targetNode = CustomNode.Create(EntityTypeNames.ObjectSign, SatellitePhonePropertyTitle);
            relation._node = CustomNode.Create(SignPropertyName, SignPropertyTitle);

            var expected = CreateGraphLink(relation, SatellitePhonePropertyTitle);

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
            var nodeTypeTitle = $"linked{typeName}";
            relation._targetNode = CustomNode.Create(typeName);
            relation._node = CustomNode.Create(typeName, nodeTypeTitle);

            var expected = CreateGraphLink(relation, nodeTypeTitle);

            var result = GraphTypeMapper.MapRelationToGraphLink(relation);

            result.Should().BeEquivalentTo(expected);
        }

        private static GraphLink CreateGraphLink(IRelation relation, string linkName)
        {
            var extra = new JObject();

            extra.Add(GraphTypeExtraPropNames.Type, relation.RelationTypeName);
            extra.Add(GraphTypeExtraPropNames.Name, linkName);

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