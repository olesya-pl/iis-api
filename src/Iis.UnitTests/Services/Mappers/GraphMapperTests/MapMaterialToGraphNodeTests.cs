using Iis.Services.Mappers.Graph;
using Xunit;
using FluentAssertions;
using Iis.Domain.Materials;
using System;
using AutoFixture.Xunit2;
using Iis.Domain.Graph;
using Newtonsoft.Json.Linq;
using System.Linq;
using Iis.Interfaces.Ontology.Schema;
using System.Collections.Generic;
using AutoFixture;

namespace Iis.UnitTests.Services.Mappers.GraphMapperTests
{
    public class MapMaterialToGraphNodeTests
    {
        [Fact]
        public void Should_ReturnNull_WhenMaterialIsNull()
        {
            Material material = null;

            var result = GraphTypeMapper.MapMaterialToGraphNode(material);

            result.Should().BeNull();
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphNode_WhenHasLinksIsTrue(Material material)
        {
            var expected = CreateGraphNode(material, true, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, true);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphNode_WhenHasLinksIsFalse(Material material)
        {
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, false);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphNode_WhenFileIsNotSet(Material material, bool hasLinks)
        {
            material.File = null;
            var expected = CreateGraphNode(material, hasLinks, GraphTypePropValues.MaterialGraphNodeNamePropValue);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, hasLinks);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphNode_WhenFileNameIsNotSet(Material material, bool hasLinks)
        {
            material.File = new File(Guid.NewGuid());
            var expected = CreateGraphNode(material, hasLinks, GraphTypePropValues.MaterialGraphNodeNamePropValue);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, hasLinks);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_MapToGraphNode_WhenFileNameIsSet(Material material, bool hasLinks, string fileName)
        {
            material.File = new File(Guid.NewGuid(), fileName);
            var expected = CreateGraphNode(material, hasLinks, fileName);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, hasLinks);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoData(EntityTypeNames.Object)]
        [InlineAutoData(EntityTypeNames.ObjectSign)]
        public void Should_HasLinksIsTrue_WhenHasGraphFeature(EntityTypeNames typeName, List<MaterialInfo> materialInfos)
        {
            var material = SetupMaterialWithFeatures(typeName, materialInfos);
            var expected = CreateGraphNode(material, true, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoData(EntityTypeNames.ObjectOfStudy)]
        [InlineAutoData(EntityTypeNames.AccessLevel)]
        [InlineAutoData(EntityTypeNames.Enum)]
        [InlineAutoData(EntityTypeNames.Event)]
        [InlineAutoData(EntityTypeNames.FuzzyDate)]
        [InlineAutoData(EntityTypeNames.Photo)]
        [InlineAutoData(EntityTypeNames.Wiki)]
        public void Should_HasLinksIsFalse_WhenHasNoGraphFeatures(EntityTypeNames typeName, List<MaterialInfo> materialInfos)
        {
            var material = SetupMaterialWithFeatures(typeName, materialInfos);
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_HasLinksIsFalse_WhenObjectsOfStudyIsNull(Material material)
        {
            material.ObjectsOfStudy = null;
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_HasLinksIsFalse_WhenObjectsOfStudyIsEmpty(Material material)
        {
            material.ObjectsOfStudy = new JObject();
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_HasLinksIsTrue_WhenObjectsOfStudyContainsNode(Material material, Guid fromNodeId)
        {
            SetupObjectsOfStudy(material, fromNodeId);
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_HasLinksIsTrue_WhenObjectsOfStudyHaveOtherNodes(Material material, Guid fromNodeId)
        {
            SetupObjectsOfStudy(material, fromNodeId, true);
            var expected = CreateGraphNode(material, true, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [RecursiveAutoData]
        public void Should_HasLinksIsFalse_WhenObjectsOfStudyHaveOnlyBaseNode(Material material, Guid fromNodeId)
        {
            SetupObjectsOfStudy(material, fromNodeId);
            var expected = CreateGraphNode(material, false, material.File.Name);

            var result = GraphTypeMapper.MapMaterialToGraphNode(material, null);

            result.Should().BeEquivalentTo(expected);
        }

        private static GraphNode CreateGraphNode(Material material, bool hasLinks, string name)
        {
            var metaDataObject = new JObject();
            metaDataObject.Add(GraphTypeExtraPropNames.Type, material.Type);
            metaDataObject.Add(GraphTypeExtraPropNames.Source, material.Source);

            var extraObject = new JObject();
            extraObject.Add(GraphTypeExtraPropNames.HasLinks, hasLinks);
            extraObject.Add(GraphTypeExtraPropNames.Type, GraphTypePropValues.MaterialGraphNodeTypePropValue);
            extraObject.Add(GraphTypeExtraPropNames.Name, name);
            extraObject.Add(GraphTypeExtraPropNames.NodeType, GraphNodeNodeTypeNames.Material);
            extraObject.Add(GraphTypeExtraPropNames.ImportanceCode, null);
            extraObject.Add(GraphTypeExtraPropNames.IconName, material.Type);
            extraObject.Add(GraphTypeExtraPropNames.MetaData, metaDataObject);

            return new GraphNode
            {
                Id = material.Id,
                Extra = extraObject
            };
        }

        private static void SetupObjectsOfStudy(Material material, Guid fromNodeId, bool generateObjects = false)
        {
            var fixture = new RecursiveAutoDataAttribute().Fixture;

            material.ObjectsOfStudy = new JObject();
            material.ObjectsOfStudy.Add(fromNodeId.ToString(), fixture.Create<string>());

            if (!generateObjects) return;

            var objectsOfStudy = fixture.Create<Dictionary<Guid, string>>();

            foreach (var (key, value) in objectsOfStudy)
            {
                material.ObjectsOfStudy.Add(key.ToString(), value);
            }
        }

        private static Material SetupMaterialWithFeatures(EntityTypeNames typeName, List<MaterialInfo> materialInfos)
        {
            var fixture = new RecursiveAutoDataAttribute().Fixture;
            var material = fixture.Create<Material>();

            material.ObjectsOfStudy = null;
            material.Infos.AddRange(materialInfos);
            material.Infos.First().Features.Add(CustomMaterialFeature.Create(typeName));

            return material;
        }
    }
}