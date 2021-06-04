using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain.Graph;
using Iis.Interfaces.Ontology.Data;
using Iis.Utility;

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
            if (relation is null) return null;

            var extra = new JObject();

            extra.Add(GraphTypeExtraPropNames.Type, relation.RelationTypeName);
            extra.Add(GraphTypeExtraPropNames.Name, GetGraphLinkNameProperty(relation));

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
            if (node is null) return null;

            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.HasLinks, DoesNodeHaveLink(node, exclusionNodeIdList));
            extraObject.Add(GraphTypeExtraPropNames.Type, $"Entity{node.NodeType.Name}");
            extraObject.Add(GraphTypeExtraPropNames.Name, GetGraphNodeNameProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.NodeType, GetGraphNodeNodeTypeProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.ImportanceCode, GetGraphNodeImportanceProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.IconName, node.NodeType.GetIconName());
            extraObject.Add(GraphTypeExtraPropNames.PhotoUrl, GetGraphNodePhotoUrl(node));

            return new GraphNode
            {
                Id = node.Id,
                Extra = extraObject
            };
        }

        private static bool DoesNodeHaveLink(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            return node.IncomingRelations.Any(e => IsEligibleForGraphByNodeType(e.SourceNode) && !exclusionNodeIdList.Contains(e.SourceNodeId))
                || node.OutgoingRelations.Any(e => IsEligibleForGraphByNodeType(e.TargetNode) && !exclusionNodeIdList.Contains(e.TargetNodeId));
        }

        private static string GetGraphNodeNameProperty(INode node) => node switch
        {
            { NodeType: { IsEvent: true } } => node.GetSingleProperty("name")?.Value,
            { NodeType: { IsObjectSign: true } } => node.GetSingleProperty("value")?.Value,
            _ => node.GetComputedValue("__title")
        };

        private static string GetGraphNodeNodeTypeProperty(INode node) => node.NodeType switch
        {
            { IsEvent: true } => GraphNodeNodeTypeNames.Event,
            { IsObject: true } => GraphNodeNodeTypeNames.Object,
            { IsObjectSign: true } => GraphNodeNodeTypeNames.Sign,
            _ => GraphNodeNodeTypeNames.Unknown
        };

        private static string GetGraphNodeImportanceProperty(INode node)
        {
            return node.GetSingleProperty("importance")?.GetSingleProperty("code")?.Value;
        }

        private static string GetGraphNodePhotoUrl(INode node)
        {
            if (node is null) return null;

            var image = node.GetChildNodes("titlePhotos").FirstOrDefault();

            if(image is null) return null;

            var imageId = new Guid(image.GetSingleProperty("image").Value);

            return FileUrlGetter.GetFileUrl(imageId);
        }

        private static string GetGraphLinkNameProperty(IRelation relation) => relation switch
        {
            { TargetNode: { NodeType: { IsObjectSign: true } } } => relation.TargetNode.NodeType.Title,
            _ => relation.Node.NodeType.Title
        };
    }
}