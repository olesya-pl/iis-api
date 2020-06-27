using System;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using IIS.Core.NodeMaterialRelation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.NodeMaterialRelation
{
    public class NodeMaterialRelationServiceTests
    {
        public readonly ServiceProvider _serviceProvider;

        public NodeMaterialRelationServiceTests()
        {
            _serviceProvider = Utils.GetServiceProvider();
        }

        [Theory, RecursiveAutoData]
        public async Task Create_SuccessPath(NodeEntity node, MaterialEntity material)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Nodes.Add(node);
            context.Materials.Add(material);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<NodeMaterialRelationService>();
            await sut.Create(new IIS.Core.NodeMaterialRelation.NodeMaterialRelation
            {
                NodeId = node.Id,
                MaterialId = material.Id
            });

            //assert
            Assert.True(context.MaterialFeatures.Any(p => p.NodeId == node.Id
                            && p.MaterialInfo.MaterialId == material.Id));
        }

        [Theory, RecursiveAutoData]
        public async Task Create_RelationAlreadyExists_Throws(NodeEntity node, MaterialEntity material)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Nodes.Add(node);
            context.Materials.Add(material);
            context.MaterialFeatures.Add(new MaterialFeatureEntity
            {
                NodeId = node.Id,
                MaterialInfo = new MaterialInfoEntity
                {
                    MaterialId = material.Id
                }
            });
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<NodeMaterialRelationService>();
            await Assert.ThrowsAsync<Exception>(() => sut.Create(new IIS.Core.NodeMaterialRelation.NodeMaterialRelation
            {
                NodeId = node.Id,
                MaterialId = material.Id
            }));

            //assert
            Assert.Equal(1, context.MaterialFeatures.Count(p => p.NodeId == node.Id
                            && p.MaterialInfo.MaterialId == material.Id));
        }

        [Theory, RecursiveAutoData]
        public async Task Delete_SuccessPath(NodeEntity node, MaterialEntity material,
            NodeEntity node2, MaterialEntity material2)
        {
            //arrange
            node.MaterialFeatures.Clear();
            node2.MaterialFeatures.Clear();
            material.MaterialInfos.Clear();
            material2.MaterialInfos.Clear();
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Nodes.AddRange(node, node2);
            context.Materials.AddRange(material, material2);
            context.MaterialFeatures.AddRange(
                new MaterialFeatureEntity
                {
                    NodeId = node.Id,
                    MaterialInfo = new MaterialInfoEntity
                    {
                        MaterialId = material.Id
                    }
                },
                new MaterialFeatureEntity
                {
                    NodeId = node2.Id,
                    MaterialInfo = new MaterialInfoEntity
                    {
                        MaterialId = material.Id
                    }
                },
                new MaterialFeatureEntity
                {
                    NodeId = node.Id,
                    MaterialInfo = new MaterialInfoEntity
                    {
                        MaterialId = material2.Id
                    }
                }
            );
            context.SaveChanges();
            //act
            var sut = _serviceProvider.GetRequiredService<NodeMaterialRelationService>();
            await sut.Delete(new IIS.Core.NodeMaterialRelation.NodeMaterialRelation
            {
                NodeId = node.Id,
                MaterialId = material.Id
            });

            //assert
            Assert.Equal(0, context.MaterialFeatures.Count(p => p.NodeId == node.Id
                            && p.MaterialInfo.MaterialId == material.Id));
            Assert.Equal(1, context.MaterialFeatures.Count(p => p.NodeId == node2.Id
                            && p.MaterialInfo.MaterialId == material.Id));
            Assert.Equal(1, context.MaterialFeatures.Count(p => p.NodeId == node.Id
                            && p.MaterialInfo.MaterialId == material2.Id));
            Assert.Equal(1, context.MaterialInfos.Count(p => p.MaterialId == material.Id));
            Assert.Equal(1, context.MaterialInfos.Count(p => p.MaterialId == material2.Id));
        }
    }
}
