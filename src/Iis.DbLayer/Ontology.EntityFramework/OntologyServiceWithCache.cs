using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyModelWrapper;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;
using IIS.Repository.Factories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Attribute = Iis.Domain.Attribute;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyServiceWithCache : IOntologyService
    {
        readonly IOntologyNodesData _data;
        readonly IElasticService _elasticService;
        public OntologyServiceWithCache(
            IOntologyNodesData data,
            IElasticService elasticService)
        {
            _data = data;
            _elasticService = elasticService;
        }
        public async Task<IEnumerable<Node>> GetEventsAssociatedWithEntity(Guid entityId)
        {
            const string propertyName = "associatedWithEvent";
            var eventType = _data.Schema.GetEntityTypeByName(EntityTypeNames.Event.ToString());
            var property = eventType.GetNodeTypeByDotNameParts(new[] { propertyName });
            if (property == null) throw new Exception($"Property does not exist: {EntityTypeNames.Event.ToString()}.{propertyName}");

            var node = _data.GetNode(entityId);
            var events = node.IncomingRelations
                .Where(r => r.Node.NodeTypeId == property.Id)
                .Select(r => r.SourceNode);

            await Task.Yield();
            return events.Select(ev => MapNode(ev)).AsEnumerable();
        }
        public async Task<List<IncomingRelation>> GetIncomingEntities(Guid entityId)
        {
            var node = _data.GetNode(entityId);
            var result = node.IncomingRelations
                .Where(r => r.RelationKind == RelationKind.Embedding)
                .Select(r => new IncomingRelation
                {
                    RelationId = r.Id,
                    RelationTypeName = r.RelationTypeName,
                    RelationTypeTitle = r.Node.NodeType.Title,
                    EntityId = r.SourceNodeId,
                    EntityTypeName = r.SourceNode.NodeType.Name,
                    Entity = MapNode(r.SourceNode)
                }).ToList();

            await Task.Yield();
            return result;
        }
        public async Task<List<Entity>> GetEntitiesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            var nodes = _data.GetNodesByUniqueValue(nodeTypeId, value, valueTypeName);

            var result = nodes
                .Select(n => (Entity)MapNode(n))
                .ToList();

            await Task.Yield();
            return result;
        }
        public async Task<Node> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            var nodes = _data.GetNodesByUniqueValue(nodeTypeId, value, valueTypeName);

            var result = nodes
                .Select(n => MapNode(n))
                .FirstOrDefault();

            await Task.Yield();
            return result;
        }
        public async Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList)
        {
            var result = _data.Relations
                .Where(r => featureIdList.Contains(r.TargetNodeId))
                .Select(r => r.SourceNodeId)
                .Distinct()
                .ToList();

            await Task.Yield();
            return result;
        }
        public async Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            var derivedTypes = _data.Schema.GetNodeTypes(types.Select(t => t.Id));
            var isElasticSearch = !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));

            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(derivedTypes.Select(t => t.Name), filter, cancellationToken);
                var nodes = _data.GetNodes(searchResult.ids);
                return nodes.Select(n => MapNode(n));
            }
            else
            {
                var nodes = GetNodesWithSuggestion(derivedTypes.Select(nt => nt.Id), filter);
                return nodes.Select(n => MapNode(n));
            }
        }
        private List<INode> GetNodesWithSuggestion(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            return _data.GetNodesByTypeIds(derived)
                .Where(n => string.IsNullOrWhiteSpace(filter.Suggestion) ||
                    n.OutgoingRelations.Any(
                        r => r.TargetNode.Value != null && 
                        r.TargetNode.Value.Contains(filter.Suggestion)))
                .Skip(filter.Offset)
                .Take(filter.Limit)
                .ToList();
        }
        private int GetNodesCountWithSuggestion(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            return _data.GetNodesByTypeIds(derived)
                .Where(n => string.IsNullOrWhiteSpace(filter.Suggestion) ||
                    n.OutgoingRelations.Any(
                        r => r.TargetNode.Value != null &&
                        r.TargetNode.Value.Contains(filter.Suggestion)))
                .Count();
        }
        public async Task<int> GetNodesCountAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            var derivedTypes = _data.Schema.GetNodeTypes(types.Select(t => t.Id));

            var isElasticSearch = !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));
            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByAllFieldsAsync(derivedTypes.Select(t => t.Name), filter);
                return searchResult.count;
            }
            else
            {
                return GetNodesCountWithSuggestion(derivedTypes.Select(nt => nt.Id), filter);
            }
        }
        public async Task<(IEnumerable<Node> nodes, int count)> GetNodesAsync(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default)
        {
            var nodes = _data.GetNodes(matchList);
            await Task.Yield();
            return (nodes.Select(n => MapNode(n)), nodes.Count);
        }
        public async Task<IReadOnlyList<IAttributeBase>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit)
        {
            IReadOnlyList<IAttributeBase> result = _data.Nodes
                .Where(n => n.NodeType.Name == valueTypeName &&
                       n.Value == value &&
                       n.OutgoingRelations.Any(r => r.SourceNode.NodeTypeId == nodeTypeId))
                .Select(n => (IAttributeBase)(n.Attribute))
                .Take(limit)
                .ToList();

            await Task.Yield();
            return result;
        }
        public async Task<Node> LoadNodesAsync(Guid nodeId, CancellationToken cancellationToken = default)
        {
            var node = _data.GetNode(nodeId);

            await Task.Yield();
            return MapNode(node);
        }
        public async Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<IEmbeddingRelationTypeModel> relationTypes, CancellationToken cancellationToken = default)
        {
            var nodes = _data.GetNodes(nodeIds.Distinct());
            await Task.Yield();
            return nodes.Select(n => MapNode(n, relationTypes)).ToList();
        }
        public async Task RemoveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            _data.RemoveNode(source.Id);
        }
        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            await SaveNodeAsync(source, null, cancellationToken);
        }
        public async Task SaveNodeAsync(Node source, Guid? requestId, CancellationToken cancellationToken = default)
        {
            _data.WriteLock(() =>
            {
                var node = _data.GetNode(source.Id) ?? _data.CreateNode(source.Type.Id, source.Id);
                SaveRelations(source, node);
            });

            if (requestId.HasValue)
            {
                await Task.WhenAll(_elasticService.PutNodeAsync(source.Id), _elasticService.PutHistoricalNodesAsync(source.Id, requestId));
            }
            else
                await _elasticService.PutNodeAsync(source.Id);
        }
        private void SaveRelations(Node source, INode existing)
        {
            foreach (IEmbeddingRelationTypeModel relationType in source.Type.AllProperties)
            {
                if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    Relation sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type.Id == relationType.Id);
                    IRelation existingRelation = existing.OutgoingRelations
                        .SingleOrDefault(e => e.Node.NodeTypeId == relationType.Id && !e.Node.IsArchived);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                else
                {
                    IEnumerable<Relation> sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type.Id == relationType.Id);
                    IEnumerable<IRelation> existingRelations = existing.OutgoingRelations.Where(e => e.Node.NodeTypeId == relationType.Id);
                    var pairs = sourceRelations.FullOuterJoin(existingRelations, e => e.Id, e => e.Id).ToList();
                    foreach (var pair in pairs)
                    {
                        ApplyChanges(existing, pair.Left, pair.Right);
                    }
                }
            }
        }
        private void ApplyChanges(INodeBase existing, Relation sourceRelation, IRelation existingRelation)
        {
            if (sourceRelation == null && existingRelation == null) return;

            if (sourceRelation == null && existingRelation != null)
            {
                _data.RemoveNode(existingRelation.Id);
            }
            else if (sourceRelation != null && existingRelation == null)
            {
                CreateRelation(sourceRelation, existing.Id);
            }
            else
            {
                Guid existingId = existingRelation.TargetNodeId;
                Guid targetId = sourceRelation.Target.Id;
                if (existingId != targetId)
                {
                    _data.RemoveNode(existingRelation.Id);
                    CreateRelation(sourceRelation, existing.Id);
                }
            }
        }
        private IRelation CreateRelation(Relation relation, Guid sourceId)
        {
            if (relation.Target is Attribute)
            {
                var attribute = relation.Target as Attribute;
                var value = AttributeType.ValueToString(attribute.Value, ((IAttributeTypeModel)attribute.Type).ScalarTypeEnum);
                return _data.CreateRelationWithAttribute(sourceId, relation.Type.Id, value);
            }
            else
            {
                return _data.CreateRelation(sourceId, relation.Target.Id, relation.Type.Id);
            }
        }
        private Node MapNode(INode node, IEnumerable<IEmbeddingRelationTypeModel> relationTypes = null)
        {
            return MapNode(node, new List<Node>(), relationTypes);
        }
        private Node MapNode(INode node, List<Node> mappedNodes, IEnumerable<IEmbeddingRelationTypeModel> relationTypes = null)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == node.Id);
            if (m != null) return m;

            var nodeType = node.NodeType;
            Node result;
            if (nodeType.Kind == Kind.Attribute)
            {
                var value = AttributeType.ParseValue(node.Value, nodeType.AttributeType.ScalarType);
                var attributeType = new AttributeTypeWrapper(nodeType);
                result = new Attribute(node.Id, attributeType, value, node.CreatedAt, node.UpdatedAt);
            }
            else if (nodeType.Kind == Kind.Entity)
            {
                var entityType = new EntityTypeWrapper(nodeType);
                result = new Entity(node.Id, entityType, node.CreatedAt, node.UpdatedAt);
                mappedNodes.Add(result);
            }
            else if (nodeType.Kind == Kind.Relation)
            {
                var relationType = new RelationTypeWrapper(nodeType);
                result = new Relation(node.Id, relationType, node.CreatedAt, node.UpdatedAt);
                var target = MapNode(node.Relation.TargetNode, mappedNodes);
                result.AddNode(target);
            }
            else throw new Exception($"Node mapping does not support ontology type {nodeType.GetType()}.");

            foreach (var relation in node.GetDirectRelations())
            {
                if (relationTypes == null || relationTypes.Any(rt => rt.Id == relation.Node.NodeType.Id))
                {
                    var mapped = MapNode(relation.Node, mappedNodes);
                    result.AddNode(mapped);
                }
            }

            if (relationTypes != null)
            {
                var inversedTypeIds = relationTypes.Where(rt => rt.IsInversed).Select(rt => rt.Id).ToList();
                if (inversedTypeIds.Count > 0)
                {
                    foreach (var relation in node.GetInversedRelations())
                    {
                        if (inversedTypeIds.Contains(relation.Node.NodeType.Id))
                        {
                            var mapped = MapNode(relation.Node, mappedNodes);
                            result.AddNode(mapped);
                        }
                    }
                }
            }

            return result;
        }
        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeAsync(IEnumerable<string> typeNameList, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            var derivedTypeNames = _data.Schema
                .GetEntityTypesByName(typeNameList, true)
                .Select(nt => nt.Name)
                .Distinct();

            var isElasticSearch = _elasticService.TypesAreSupported(derivedTypeNames);
            if (isElasticSearch)
            {
                return await _elasticService.SearchEntitiesByConfiguredFieldsAsync(derivedTypeNames, filter);
            }
            else
            {
                return new SearchEntitiesByConfiguredFieldsResult();
            }
        }
        public string GetAttributeValueByDotName(Guid id, string dotName)
        {
            var node = _data.GetNode(id);
            return node?.GetSingleProperty(dotName)?.Value;
        }
    }
}
