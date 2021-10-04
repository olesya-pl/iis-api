using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Mappers.Graph;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
namespace Iis.Services
{
    public class GraphService : IGraphService
    {
        private readonly IOntologyNodesData _data;
        private readonly IMaterialProvider _materialProvider;

        public GraphService(IOntologyNodesData data, IMaterialProvider materialProvider)
        {
            _data = data;
            _materialProvider = materialProvider;
        }

        public async Task<GraphData> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> idList)
        {
            var graphData = new GraphData();

            foreach (var id in idList)
            {
                var node = _data.GetNode(id);
                if (node != null)
                {
                    graphData.AddData(await GetGraphDataForNodeAsync(node));
                    continue;
                }

                var material = await _materialProvider.GetMaterialAsync(id);
                if (material != null)
                {
                    graphData.AddData(GetGraphDataForMaterial(material));
                }
            }

            return graphData;
        }

        private static IReadOnlyCollection<GraphLink> GetGraphLinkListForNode(INode node)
        {
            if(node is null) return Array.Empty<GraphLink>();

            var incomingLinkList = node.IncomingRelations
                                    .Where(e => GraphTypeMapper.IsEligibleForGraphByNodeType(e.SourceNode))
                                    .Select(e => GraphTypeMapper.MapRelationToGraphLink(e))
                                    .ToArray();
            var outgoingLinkList = node.OutgoingRelations
                                    .Where(e => GraphTypeMapper.IsEligibleForGraphByNodeType(e.TargetNode))
                                    .Select(e => GraphTypeMapper.MapRelationToGraphLink(e))
                                    .ToArray();

            var result = new List<GraphLink>(incomingLinkList.Length + outgoingLinkList.Length);

            result.AddRange(incomingLinkList);

            result.AddRange(outgoingLinkList);

            return result.ToArray();
        }

        private static IReadOnlyCollection<GraphNode> GetGraphNodeListForNode(INode node)
        {
            if(node is null) return Array.Empty<GraphNode>();

            var exclusionNodeIdList = new []{node.Id};

            var incomingNodeList = node.IncomingRelations
                                    .Select(e => e.SourceNode)
                                    .Where(GraphTypeMapper.IsEligibleForGraphByNodeType)
                                    .Select(e => GraphTypeMapper.MapNodeToGraphNode(e, exclusionNodeIdList))
                                    .ToArray();

            var outgoingNodeList = node.OutgoingRelations
                                    .Select(e => e.TargetNode)
                                    .Where(GraphTypeMapper.IsEligibleForGraphByNodeType)
                                    .Select(e => GraphTypeMapper.MapNodeToGraphNode(e, exclusionNodeIdList))
                                    .ToArray();


            var result = new List<GraphNode>(incomingNodeList.Length + outgoingNodeList.Length + 1);

            var rootNodeExclusionList = new List<Guid>(incomingNodeList.Length + outgoingNodeList.Length);

            rootNodeExclusionList.AddRange(incomingNodeList.Select(e => e.Id));
            rootNodeExclusionList.AddRange(outgoingNodeList.Select(e => e.Id));

            result.Add(GraphTypeMapper.MapNodeToGraphNode(node, rootNodeExclusionList));
            result.AddRange(incomingNodeList);
            result.AddRange(outgoingNodeList);

            return result.ToArray();
        }

        private static IReadOnlyCollection<GraphLink> GetGraphLinksForMaterials(IReadOnlyCollection<Material> materialList, INode node)
        {
            if(!materialList.Any()) return Array.Empty<GraphLink>();

            return materialList
                .Select(e => GraphTypeMapper.MapMaterialToGraphLink(e, node.Id))
                .ToArray();
        }

        private static IReadOnlyCollection<GraphNode> GetGraphNodesForMaterials(IReadOnlyCollection<Material> materialList, INode node)
        {
            if(!materialList.Any()) return Array.Empty<GraphNode>();

            return materialList
                .Select(e => GraphTypeMapper.MapMaterialToGraphNode(e, null, node.Id))
                .ToArray();
        }

        private async Task<GraphData> GetGraphDataForNodeAsync(INode node)
        {
            var graphData = new GraphData();

            var materialResult = await _materialProvider.GetMaterialsByNodeIdAndRelatedEntities(node.Id);

            var materialList = materialResult.Materials.ToArray();

            graphData.AddLinks(GetGraphLinkListForNode(node));

            graphData.AddLinks(GetGraphLinksForMaterials(materialList, node));

            graphData.AddNodes(GetGraphNodeListForNode(node));

            graphData.AddNodes(GetGraphNodesForMaterials(materialList, node));

            return graphData;
        }

        private GraphData GetGraphDataForMaterial(Material material)
        {
            var graphData = new GraphData();

            var nodes = material.Infos
                .SelectMany(m => m.Features)
                .Where(f => f.Node != null && GraphTypeMapper.IsEligibleForGraphByNodeType(f.Node.OriginalNode))
                .Select(f => f.Node.OriginalNode);

            var exclusionNodeIdList = new Guid[] { };

            graphData.AddNode(GraphTypeMapper.MapMaterialToGraphNode(material, false));

            foreach (var node in nodes)
            {
                graphData.AddLink(GraphTypeMapper.MapMaterialToNodeGraphLink(material, node));
                graphData.AddNode(GraphTypeMapper.MapNodeToGraphNode(node, exclusionNodeIdList));
            }

            return graphData;
        }
    }
}