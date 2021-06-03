using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain.Graph;
using Iis.Interfaces.Ontology.Data;
namespace Iis.Services.Graph
{
    public static class GraphTypeMapper
    {
        public static bool IsEligibleForGraphByNodeType(INode node)
        {
            return node.NodeType.IsObject || node.NodeType.IsObjectSign;
        }

        public static GraphLink MapRelationToGraphLink(IRelation relation)
        {
            if(relation is null) return null;

            var extra = new JObject();

            extra.Add(GraphTypeExtraPropNames.Type, relation.RelationTypeName);
            extra.Add(GraphTypeExtraPropNames.Name, relation.Node.NodeType.Title);

            return new GraphLink
            {
                Id = relation.Id,
                From = relation.SourceNodeId,
                To = relation.TargetNodeId,
                Extra = extra
            };
        }

        public static GraphNode MapNodeToGraphNode(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            if(node is null) return null;
 
            var extraJObject = new JObject();

            extraJObject.Add(GraphTypeExtraPropNames.HasLinks, DoesNodeHaveLink(node, exclusionNodeIdList));
            extraJObject.Add(GraphTypeExtraPropNames.Type, $"Entity{node.NodeType.Name}");
            extraJObject.Add(GraphTypeExtraPropNames.Name, GetNameProperty(node));

            return new GraphNode
            {
                Id = node.Id,
                Extra = extraJObject
            };
        }

        private static bool DoesNodeHaveLink(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            return node.IncomingRelations.Any(e => IsEligibleForGraphByNodeType(e.SourceNode) && !exclusionNodeIdList.Contains(e.SourceNodeId)) || node.OutgoingRelations.Any(e => IsEligibleForGraphByNodeType(e.TargetNode) && !exclusionNodeIdList.Contains(e.TargetNodeId));
        }

        private static string GetNameProperty(INode node) => node switch
        {
            var e when e.NodeType.IsEvent => node.GetSingleProperty("name")?.Value,
            _ => node.GetComputedValue("__title")
        };
    }
}