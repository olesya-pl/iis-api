using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class ExtNodeService: IExtNodeService
    {
        private const string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ssZ";
        private readonly IReadOnlyCollection<string> _filterNodeTypeNames = new [] {"__title", "title", "lastConfirmedAt"};

        public IExtNode GetExtNode(INode node)
        {
            var extNode = MapExtNode(
                node,
                node.NodeType.Name,
                node.NodeType.Title,
                new List<Guid> { node.Id });
            return extNode;
        }

        public List<IExtNode> GetExtNodes(IReadOnlyCollection<INode> ids)
        {
            return ids.Select(p => GetExtNode(p)).ToList();
        }

        private async Task<ExtNode> MapExtNodeWithoutChildRelationsAsync(
            INode node,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedNodeIds,
            CancellationToken cancellationToken = default)
        {
            if (node.NodeType.IsObject)
            {
                visitedNodeIds.Add(node.Id);
            }
            var extNode = MapExtNodeBase(node, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = node.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(node);
            extNode.ScalarType = node.NodeType?.AttributeType?.ScalarType;
            extNode.Children = await GetExtNodesByRelations(node.OutgoingRelations, visitedNodeIds, cancellationToken);
            return extNode;
        }

        private async Task<ExtNode> MapExtNodeAsync(
            INode node,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedNodeIds,
            CancellationToken cancellationToken = default)
        {
            if (node.NodeType.IsObject)
            {
                visitedNodeIds.Add(node.Id);
            }
            var extNode = MapExtNodeBase(node, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = node.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(node);
            extNode.ScalarType = node.NodeType?.AttributeType?.ScalarType;
            extNode.Children = await GetExtNodesByRelations(node.OutgoingRelations, visitedNodeIds, cancellationToken);
            return extNode;
        }
        private ExtNode MapExtNode(
            INode node,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedNodeIds,
            IReadOnlyCollection<string> filterRelationList = null)
        {
            if (node.NodeType.IsObject)
            {
                visitedNodeIds.Add(node.Id);
            }
            var extNode = MapExtNodeBase(node, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = node.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(node);
            extNode.ScalarType = node.NodeType?.AttributeType?.ScalarType;

            IReadOnlyCollection<IRelation> filteredRelations = node.OutgoingRelations;

            if(filterRelationList != null && filterRelationList.Any())
            {
                filteredRelations = filteredRelations
                                    .Where(e => _filterNodeTypeNames.Contains(e.RelationTypeName))
                                    .ToArray();
            }

            var children = MapComputedProperties(node, node.NodeType.GetComputedRelationTypes());
            children.AddRange(GetExtNodesByRelations(filteredRelations, visitedNodeIds));
            extNode.Children = children;
            return extNode;
        }

        private List<ExtNode> MapComputedProperties(INode node, IEnumerable<IRelationTypeLinked> computedRelationTypes)
        {
            return computedRelationTypes.Select(relationType => {
                var formula = relationType.NodeType.MetaObject.Formula;
                var computedValue = node.ResolveFormula(formula);
                return new ExtNode
                {
                    NodeTypeId = relationType.Id.ToString("N"),
                    NodeType = relationType.NodeType,
                    NodeTypeName = relationType.NodeType.Name,
                    NodeTypeTitle = relationType.NodeType.Title,
                    EntityTypeName = relationType.SourceType.Name,
                    AttributeValue = computedValue,
                    ScalarType = ScalarType.String
                };
            }).ToList();
        }


        private ExtNode MapExtNodeWithoutChildRelations(
            INode node,
            string nodeTypeName,
            string nodeTypeTitle,
            List<Guid> visitedNodeIds)
        {
            if (node.NodeType.IsObject)
            {
                visitedNodeIds.Add(node.Id);
            }
            var extNode = MapExtNodeBase(node, nodeTypeName, nodeTypeTitle);
            extNode.EntityTypeName = node.NodeType.Name;
            extNode.AttributeValue = GetAttributeValue(node);
            extNode.ScalarType = node.NodeType?.AttributeType?.ScalarType;
            return extNode;
        }

        private ExtNode MapExtNodeBase(
            INode node,
            string nodeTypeName,
            string nodeTypeTitle)
        {
            return new ExtNode
            {
                Id = node.Id.ToString("N"),
                NodeTypeId = node.NodeTypeId.ToString("N"),
                NodeType = node.NodeType,
                NodeTypeName = nodeTypeName,
                NodeTypeTitle = nodeTypeTitle,
                CreatedAt = node.CreatedAt,
                UpdatedAt = node.UpdatedAt
            };
        }

        private object GetAttributeValue(INode node)
        {
            if (node.Attribute == null) return null;

            var scalarType = node.NodeType.AttributeType.ScalarType;
            var value = node.Attribute.Value;
            return FormatValue(scalarType, value);
        }

        private object FormatValue(ScalarType scalarType, string value)
        {
            switch (scalarType)
            {
                case ScalarType.Int:
                    return Convert.ToInt32(value);
                case ScalarType.Date:
                    {
                        if (DateTime.TryParse(value, out DateTime dateTimeValue))
                        {
                            return dateTimeValue.ToString(Iso8601DateFormat);
                        }
                        else
                        {
                            return value;
                        }
                    }
                case ScalarType.File:
                    return new
                    {
                        fileId = value,
                        url = FileUrlGetter.GetFileUrl(new Guid(value))
                    };
                case ScalarType.IntegerRange:
                case ScalarType.FloatRange:
                    return FormatRange(value);
                default:
                    return value;
            }
        }

        private object FormatRange(string value)
        {
            var splitted = value.Split('-', ' ', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Count() == 1 || splitted.Count() == 2)
            {
                var firstString = splitted.First();
                var lastString = splitted.Last();

                if (decimal.TryParse(firstString, out var first) && decimal.TryParse(lastString, out var last))
                {
                    return new
                    {
                        gte = first,
                        lte = last
                    };
                }
            }
            return null;
        }

        private async Task<List<ExtNode>> GetExtNodesByRelations(
            IEnumerable<IRelation> relations,
            List<Guid> visitedNodeIds,
            CancellationToken cancellationToken = default)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                if (!visitedNodeIds.Contains(relation.TargetNodeId))
                {
                    var extNode = await MapExtNodeAsync(
                        relation.TargetNode,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        visitedNodeIds,
                        cancellationToken);
                    result.Add(extNode);

                    var meta = relation.Node.NodeType.Meta;
                    if (!string.IsNullOrEmpty(meta))
                    {
                        var metaObj = JObject.Parse(meta);
                        if (metaObj.ContainsKey("IsAggregated") && metaObj.Value<bool>("IsAggregated") == true)
                        {
                            var aggregateNode = await MapExtNodeWithoutChildRelationsAsync(
                                relation.TargetNode,
                                $"{relation.Node.NodeType.Name}Aggregate",
                                relation.Node.NodeType.Title,
                                visitedNodeIds);
                            result.Add(aggregateNode);
                        }
                    }
                }
            }
            return result;
        }
        private List<ExtNode> GetExtNodesByRelations(
            IReadOnlyCollection<IRelation> relations,
            List<Guid> visitedNodeIds)
        {
            var result = new List<ExtNode>();
            foreach (var relation in relations.Where(r => !r.Node.IsArchived && !r.Node.NodeType.IsArchived
                && !r.TargetNode.IsArchived && !r.TargetNode.NodeType.IsArchived))
            {
                var node = relation.TargetNode;

                if (!visitedNodeIds.Contains(node.Id))
                {
                    IReadOnlyCollection<string> relationFitler = null;

                    if(node.NodeType.IsObject)
                    {
                        relationFitler = _filterNodeTypeNames;
                    }

                    var extNode = MapExtNode(
                        node,
                        relation.Node.NodeType.Name,
                        relation.Node.NodeType.Title,
                        visitedNodeIds,
                        relationFitler);

                    result.Add(extNode);

                    if (relation.Node.NodeType.MetaObject.IsAggregated == true)
                    {
                        var aggregateNode = MapExtNodeWithoutChildRelations(
                            node,
                            $"{relation.Node.NodeType.Name}Aggregate",
                            relation.Node.NodeType.Title,
                            visitedNodeIds);
                        result.Add(aggregateNode);
                    }
                 }
            }
            return result;
        }
    }
}
