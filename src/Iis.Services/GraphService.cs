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
                                    .Select(GraphTypeMapper.MapRelationToGraphLink)
                                    .ToArray();
            var outgoingLinkList = node.OutgoingRelations
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

            var incomingNodeList = node.IncomingRelations
                                    .Select(e => e.SourceNode)
                                    .Select(GraphTypeMapper.MapNodeToGraphNode)
                                    .ToArray();

            var outgoingNodeList = node.OutgoingRelations
                                    .Select(e => e.TargetNode)
                                    .Select(GraphTypeMapper.MapNodeToGraphNode)
                                    .ToArray();


            var result = new List<GraphNode>(incomingNodeList.Length + outgoingNodeList.Length);

            result.AddRange(incomingNodeList);
            result.AddRange(outgoingNodeList);

            return result.ToArray();
        }
    }
}