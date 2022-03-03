using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Interfaces.Ontology.Comparers;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Mappers.Graph;

namespace Iis.Services
{
    public class GraphService : IGraphService
    {
        private static ISecurityLevelChecker _securityLevelChecker;
        private readonly IOntologyNodesData _data;
        private readonly IMaterialProvider _materialProvider;

        public GraphService(
            IOntologyNodesData data,
            IMaterialProvider materialProvider,
            ISecurityLevelChecker securityLevelChecker)
        {
            _data = data;
            _materialProvider = materialProvider;
            _securityLevelChecker = securityLevelChecker;
        }

        public async Task<GraphData> GetGraphDataForNodeListAsync(IReadOnlyCollection<Guid> nodeIdList, User user, CancellationToken cancellationToken)
        {
            var graphData = new GraphData();

            foreach (var id in nodeIdList)
            {
                var node = _data.GetNode(id);
                if (node != null)
                {
                    graphData.AddData(await GetGraphDataForNodeAsync(node, user));
                    continue;
                }

                var material = await _materialProvider.GetMaterialAsync(id);
                if (material != null)
                {
                    graphData.AddData(GetGraphDataForMaterial(material, user));
                }
            }

            return graphData;
        }

        private static IReadOnlyCollection<GraphLink> GetGraphLinkListForNode(INode node)
        {
            if (node is null) return Array.Empty<GraphLink>();

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

        private static IReadOnlyCollection<GraphNode> GetGraphNodeListForNode(INode node, User user)
        {
            if (node is null) return Array.Empty<GraphNode>();

            Guid[] exclusionNodeIdList = { node.Id };

            var incomingNodeList = node.IncomingRelations
                                    .Select(e => e.SourceNode)
                                    .Where(GraphTypeMapper.IsEligibleForGraphByNodeType)
                                    .Select(e => GraphTypeMapper.MapNodeToGraphNode(e, exclusionNodeIdList, IsAllowedEntityForUser(e, user)))
                                    .ToArray();

            var outgoingNodeList = node.OutgoingRelations
                                    .Select(e => e.TargetNode)
                                    .Where(GraphTypeMapper.IsEligibleForGraphByNodeType)
                                    .Select(e => GraphTypeMapper.MapNodeToGraphNode(e, exclusionNodeIdList, IsAllowedEntityForUser(e, user)))
                                    .ToArray();

            var result = new List<GraphNode>(incomingNodeList.Length + outgoingNodeList.Length + 1);

            var rootNodeExclusionList = new List<Guid>(incomingNodeList.Length + outgoingNodeList.Length);

            rootNodeExclusionList.AddRange(incomingNodeList.Select(e => e.Id));
            rootNodeExclusionList.AddRange(outgoingNodeList.Select(e => e.Id));

            result.Add(GraphTypeMapper.MapNodeToGraphNode(node, rootNodeExclusionList, IsAllowedEntityForUser(node, user)));
            result.AddRange(incomingNodeList);
            result.AddRange(outgoingNodeList);

            return result.ToArray();
        }

        private static IReadOnlyCollection<GraphLink> GetGraphLinksForMaterials(IReadOnlyCollection<Material> materialList, INode node)
        {
            if (!materialList.Any()) return Array.Empty<GraphLink>();

            return materialList
                .Select(e => GraphTypeMapper.MapRelatedMaterialGraphLink(e, node.Id))
                .ToArray();
        }

        private static IReadOnlyCollection<GraphNode> GetGraphNodesForMaterials(IReadOnlyCollection<Material> materialList, INode node, User user)
        {
            if (!materialList.Any()) return Array.Empty<GraphNode>();

            return materialList
                .Select(e => GraphTypeMapper.MapMaterialToGraphNode(e, IsAllowedMaterialForUser(e, user), null, node.Id))
                .ToArray();
        }

        private static GraphData GetGraphDataForMaterial(Material material, User user)
        {
            var graphData = new GraphData();

            var nodes = material.Infos
                .SelectMany(m => m.Features)
                .Where(f => f.Node != null && GraphTypeMapper.IsEligibleForGraphByNodeType(f.Node.OriginalNode))
                .Select(f => f.Node.OriginalNode)
                .Distinct(NodeByIdComparer.Instance);

            var exclusionNodeIdList = Array.Empty<Guid>();

            graphData.AddNode(GraphTypeMapper.MapMaterialToGraphNode(material, IsAllowedMaterialForUser(material, user), false));

            foreach (var node in nodes)
            {
                graphData.AddLink(GraphTypeMapper.MapRelatedFromMaterialNodeGraphLink(material, node));
                graphData.AddNode(GraphTypeMapper.MapNodeToGraphNode(node, exclusionNodeIdList, IsAllowedEntityForUser(node, user)));
            }

            return graphData;
        }

        private static bool IsAllowedEntityForUser(INode node, User user)
        {
            return _securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes, node.GetSecurityLevelIndexes());
        }

        private static bool IsAllowedMaterialForUser(Material material, User user)
        {
            var materialSecurityLevelIndexes = material.SecurityLevels.Select(_ => _.UniqueIndex).ToList();

            return _securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes, materialSecurityLevelIndexes);
        }

        private async Task<GraphData> GetGraphDataForNodeAsync(INode node, User user)
        {
            var graphData = new GraphData();

            var materialResult = await _materialProvider.GetMaterialsByNodeIdAndRelatedEntities(node.Id);

            var materialList = materialResult.Materials.ToArray();

            graphData.AddLinks(GetGraphLinkListForNode(node));

            graphData.AddLinks(GetGraphLinksForMaterials(materialList, node));

            graphData.AddNodes(GetGraphNodeListForNode(node, user));

            graphData.AddNodes(GetGraphNodesForMaterials(materialList, node, user));

            return graphData;
        }
    }
}