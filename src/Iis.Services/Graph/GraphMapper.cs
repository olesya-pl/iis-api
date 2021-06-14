using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
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

        public static GraphLink MapMaterialToGraphLink(Material material, Guid fromNodeId)
        {
            if (material is null) return null;

            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.Type, GraphTypePropValues.MaterialGraphLinkTypePropValue);
            extraObject.Add(GraphTypeExtraPropNames.Name, GraphTypePropValues.MaterialGraphLinkNamePropValue);

            return new GraphLink
            {
                Id = Guid.NewGuid(),
                From = fromNodeId,
                To = material.Id,
                Extra = extraObject
            };
        }

        public static GraphNode MapNodeToGraphNode(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            if (node is null) return null;

            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.HasLinks, DoesNodeHaveLinks(node, exclusionNodeIdList));
            extraObject.Add(GraphTypeExtraPropNames.Type, $"Entity{node.NodeType.Name}");
            extraObject.Add(GraphTypeExtraPropNames.Name, GetGraphNodeNameProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.NodeType, GetGraphNodeNodeTypeProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.ImportanceCode, GetGraphNodeImportanceProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.IconName, GetGraphNodeIconNameProperty(node));
            extraObject.Add(GraphTypeExtraPropNames.PhotoUrl, GetGraphNodePhotoUrl(node));

            return new GraphNode
            {
                Id = node.Id,
                Extra = extraObject
            };
        }

        public static GraphNode MapMaterialToGraphNode(Material material, Guid fromNodeId)
        {
            if(material is null) return null;

            var extraObject = new JObject();

            var metaDataObject = new JObject();

            metaDataObject.Add(GraphTypeExtraPropNames.Type, material.Type);
            metaDataObject.Add(GraphTypeExtraPropNames.Source, material.Source);

            extraObject.Add(GraphTypeExtraPropNames.HasLinks, DoesMaterialHaveLinks(material, fromNodeId));
            extraObject.Add(GraphTypeExtraPropNames.Type, GraphTypePropValues.MaterialGraphNodeTypePropValue);
            extraObject.Add(GraphTypeExtraPropNames.Name, material.File?.Name ?? GraphTypePropValues.MaterialGraphNodeNamePropValue);
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

        private static bool DoesNodeHaveLinks(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            if(node is null) return false;

            return node.IncomingRelations.Any(e => IsEligibleForGraphByNodeType(e.SourceNode) && !exclusionNodeIdList.Contains(e.SourceNodeId))
                || node.OutgoingRelations.Any(e => IsEligibleForGraphByNodeType(e.TargetNode) && !exclusionNodeIdList.Contains(e.TargetNodeId));
        }

        private static bool DoesMaterialHaveLinks(Material material, Guid nodeId)
        {
            return false;
            //TEMPORALY COMMENTED
            /*
            if(material is null) return false;

            if(material.ObjectsOfStudy is null || !material.ObjectsOfStudy.HasValues) return false;

            var nodeIdList = material.ObjectsOfStudy.Properties()
                                .Select(p => p.Name)
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(p => new Guid(p))
                                .Where(p => !p.Equals(nodeId))
                                .ToArray();

            return nodeIdList.Any();
            */
        }

        private static string GetGraphNodeNameProperty(INode node) => node switch
        {
            { NodeType: { IsEvent: true } } => node.GetSingleProperty(UsedNodePropNames.Name)?.Value,
            { NodeType: { IsObjectSign: true } } => node.GetSingleProperty(UsedNodePropNames.Value)?.Value,
            _ => node.GetComputedValue(UsedNodePropNames.Title)
        };

        private static string GetGraphNodeNodeTypeProperty(INode node) => node.NodeType switch
        {
            { IsEvent: true, IsObject: true } => GraphNodeNodeTypeNames.Event,
            { IsObject: true, IsEvent: false } => GraphNodeNodeTypeNames.Object,
            { IsObjectSign: true } => GraphNodeNodeTypeNames.Sign,
            _ => GraphNodeNodeTypeNames.Unknown
        };

        private static string GetGraphNodeImportanceProperty(INode node)
        {
            return node.GetSingleProperty(UsedNodePropNames.Importance)?.GetSingleProperty(UsedNodePropNames.Code)?.Value;
        }

        private static string GetGraphNodeIconNameProperty(INode node) => node.NodeType switch
        {
            { IsEvent: true, Name: var name } => name,
            { IsObjectSign: true, Name: var name } => name,
            _ => node.NodeType.GetIconName()
        };

        private static string GetGraphNodePhotoUrl(INode node)
        {
            if (node is null) return null;

            var image = node.GetChildNodes(UsedNodePropNames.TitlePhotos).FirstOrDefault();

            if (image is null) return null;

            var imageId = new Guid(image.GetSingleProperty(UsedNodePropNames.Image).Value);

            return FileUrlGetter.GetFileUrl(imageId);
        }

        private static string GetGraphLinkNameProperty(IRelation relation) => relation switch
        {
            { TargetNode: { NodeType: { IsObjectSign: true } } } => relation.TargetNode.NodeType.Title,
            _ => relation.Node.NodeType.Title
        };
    }
}