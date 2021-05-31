using System.Linq;
using Iis.Domain.Graph;
using Iis.Interfaces.Ontology.Data;
using Newtonsoft.Json.Linq;
namespace Iis.Services.Graph
{
    public static class GraphTypeMapper
    {
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

        public static GraphNode MapNodeToGraphNode(INode node)
        {
            if(node is null) return null;

            return new GraphNode
            {
                Id = node.Id,
                Extra = MapNodeToExtraJObject(node)
            };
        }

        private static JObject MapNodeToExtraJObject(INode node)
        {
            var result = new JObject();

            result.Add(GraphTypeExtraPropNames.HasLinks, DoesNodeHaveLink(node));
            result.Add(GraphTypeExtraPropNames.Type, node.NodeType.Name);
            result.Add(GraphTypeExtraPropNames.Name, GetNameFromNode(node));

            return result;
        }

        private static bool DoesNodeHaveLink(INode node)
        {
            return node.IncomingRelations.Any() || node.OutgoingRelations.Any();
        }

        private static string GetNameFromNode(INode node)
        {
            return node.Value ?? node.GetComputedValue("__title") ?? node.GetSingleProperty("name")?.Value;
        }
    }
}