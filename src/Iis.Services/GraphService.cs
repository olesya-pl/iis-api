using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain.Graph;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Graph;
using Iis.Services.Contracts.Interfaces;
namespace Iis.Services
{
    public class GraphService : IGraphService
    {
        private readonly IOntologyNodesData _data;
        public GraphService(IOntologyNodesData data)
        {
            _data = data;
        }

        public Task<(IReadOnlyCollection<GraphLink> LinkList, IReadOnlyCollection<GraphNode> NodeList)> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, IReadOnlyCollection<Guid> relationTypeList)
        {
            var nodeList = _data.GetNodes(nodeIdList);

            var graphLinkList = GetGraphLinkListForNode(nodeList.First());

            var graphNodeList = GetGraphNodeListForNode(nodeList.First());

            return Task.FromResult<(IReadOnlyCollection<GraphLink> LinkList, IReadOnlyCollection<GraphNode> NodeList)>((graphLinkList, graphNodeList));
        }

        private static IReadOnlyCollection<GraphLink> GetGraphLinkListForNode(INode node)
        {
            if(node is null) return Array.Empty<GraphLink>();

            var incomingLinkList = node.IncomingRelations
                                    .Where(e => GraphTypeMapper.IsEligibleForGraphByNodeType(e.SourceNode))
                                    .Select(GraphTypeMapper.MapRelationToGraphLink)
                                    .ToArray();
            var outgoingLinkList = node.OutgoingRelations
                                    .Where(e => GraphTypeMapper.IsEligibleForGraphByNodeType(e.TargetNode))
                                    .Select(GraphTypeMapper.MapRelationToGraphLink)
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
    }
}