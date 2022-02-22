using System;
using System.Linq;
using System.Collections.Generic;
using Iis.Domain;
using Newtonsoft.Json.Linq;
using Iis.Utility;
using Iis.Domain.Graph;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Services.Mappers.Graph
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

        public static GraphLink MapRelatedMaterialGraphLink(Material material, Guid fromNodeId)
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

        public static GraphLink MapRelatedFromMaterialNodeGraphLink(Material material, INode node)
        {
            if (material is null || node is null) return null;

            var extraObject = new JObject();

            extraObject.Add(GraphTypeExtraPropNames.Type, GetTypePropertyForRelatedFromMaterialNode(node));
            extraObject.Add(GraphTypeExtraPropNames.Name, node.NodeType.Title);

            return new GraphLink
            {
                Id = Guid.NewGuid(),
                From = material.Id,
                To = node.Id,
                Extra = extraObject
            };
        }

        public static GraphNode MapNodeToGraphNode(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList, ISecurityLevelChecker securityLevelChecker, IOntologyService ontologyService, User user)
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
            extraObject.Add(GraphTypeExtraPropNames.AccessAllowed, IsAllowedEntityForUser(node.Id, securityLevelChecker, ontologyService, user));

            return new GraphNode
            {
                Id = node.Id,
                Extra = extraObject
            };
        }

        public static GraphNode MapMaterialToGraphNode(Material material, ISecurityLevelChecker securityLevelChecker, IOntologyService ontologyService, User user, bool? hasLinks = null, Guid fromNodeId = default)
        {
            if (material is null) return null;

            var extraObject = new JObject();

            var metaDataObject = new JObject();

            metaDataObject.Add(GraphTypeExtraPropNames.Type, material.Type);
            metaDataObject.Add(GraphTypeExtraPropNames.Source, material.Source);

            extraObject.Add(GraphTypeExtraPropNames.HasLinks, hasLinks ?? DoesMaterialHaveLinks(material, fromNodeId));
            extraObject.Add(GraphTypeExtraPropNames.Type, GraphTypePropValues.MaterialGraphNodeTypePropValue);
            extraObject.Add(GraphTypeExtraPropNames.Name, material.File?.Name ?? GraphTypePropValues.MaterialGraphNodeNamePropValue);
            extraObject.Add(GraphTypeExtraPropNames.NodeType, GraphNodeNodeTypeNames.Material);
            extraObject.Add(GraphTypeExtraPropNames.ImportanceCode, null);
            extraObject.Add(GraphTypeExtraPropNames.IconName, material.Type);
            extraObject.Add(GraphTypeExtraPropNames.MetaData, metaDataObject);
            extraObject.Add(GraphTypeExtraPropNames.AccessAllowed, IsAllowedMaterialForUser(material, securityLevelChecker, user));

            return new GraphNode
            {
                Id = material.Id,
                Extra = extraObject
            };
        }

        private static bool DoesNodeHaveLinks(INode node, IReadOnlyCollection<Guid> exclusionNodeIdList)
        {
            if (node is null) return false;

            return node.IncomingRelations.Any(e => IsEligibleForGraphByNodeType(e.SourceNode) && !exclusionNodeIdList.Contains(e.SourceNodeId))
                || node.OutgoingRelations.Any(e => IsEligibleForGraphByNodeType(e.TargetNode) && !exclusionNodeIdList.Contains(e.TargetNodeId));
        }

        private static bool DoesMaterialHaveLinks(Material material, Guid nodeId)
        {
            if (material is null) return false;

            if (DoesMaterialHaveGraphFeatures(material)) return true;

            if (material.ObjectsOfStudy is null || !material.ObjectsOfStudy.HasValues) return false;

            var nodeIdList = material.ObjectsOfStudy.Properties()
                                .Select(p => p.Name)
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(p => new Guid(p))
                                .Where(p => !p.Equals(nodeId))
                                .ToArray();

            return nodeIdList.Any();
        }

        private static bool DoesMaterialHaveGraphFeatures(Material material) => material.Infos
                .SelectMany(m => m.Features).Any(f => f.Node != null && IsEligibleForGraphByNodeType(f.Node.OriginalNode));

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
            { TargetNode: { NodeType: { IsObjectSign: true } } } => $"{relation.TargetNode.NodeType.Title} [{relation.Node.NodeType.Title}]",
            _ => relation.Node.NodeType.Title
        };

        private static string GetTypePropertyForRelatedFromMaterialNode(INode node) => node.NodeType switch
        {
            { IsObjectSign: true } => GraphNodeNodeTypeNames.Sign,
            _ => $"related{node.NodeType.Name}"
        };

        private static bool IsAllowedEntityForUser(Guid id, ISecurityLevelChecker securityLevelChecker, IOntologyService ontologyService, User user)
        {
            return securityLevelChecker.AccessGranted(
                user.SecurityLevelsIndexes,
                ontologyService.GetNode(id).OriginalNode.GetSecurityLevelIndexes());
        }

        private static bool IsAllowedMaterialForUser(Material material, ISecurityLevelChecker securityLevelChecker, User user)
        {
            var materialSecurityLevelIndexes = material.SecurityLevels.Select(_ => _.UniqueIndex).ToList();

            return securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes, materialSecurityLevelIndexes);
        }
    }
}