using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Graph;
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

        public async Task<(IReadOnlyCollection<GraphLink> LinkList, IReadOnlyCollection<GraphNode> NodeList)> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, IReadOnlyCollection<Guid> relationTypeList)
        {
            var nodeList = _data.GetNodes(nodeIdList);

            var graphLinkList  = new List<GraphLink>();

            var graphNodeList = new List<GraphNode>();

            foreach (INode node in nodeList)
            {
                var materialResult = await _materialProvider.GetMaterialsByNodeIdQuery(node.Id);

                var materialList = materialResult.Materials.ToArray();

                graphLinkList.AddRange(GetGraphLinkListForNode(node));

                graphLinkList.AddRange(GetGraphLinksForMaterials(materialList, node));

                graphNodeList.AddRange(GetGraphNodeListForNode(node));

                graphNodeList.AddRange(GetGraphNodesForMaterials(materialList, node));
            }

            return (graphLinkList, graphNodeList);
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
                .Select(e => GraphTypeMapper.MapMaterialToGraphNode(e, node.Id))
                .ToArray();
        }
    }
}