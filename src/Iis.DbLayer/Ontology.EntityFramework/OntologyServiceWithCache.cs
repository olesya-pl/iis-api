using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyModelWrapper;
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
        public async Task<(IEnumerable<JObject> nodes, int count)> FilterObjectsOfStudyAsync(ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
                return nodes.Select(MapNode);
            }
            else
            {
                var nodes = GetNodesWithSuggestion(derivedTypes.Select(nt => nt.Id), filter);
                return nodes.Select(MapNode);
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
            return (nodes.Select(MapNode), nodes.Count);
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

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<IRelationTypeModel> toLoad, CancellationToken cancellationToken = default)
        {
            var node = _data.GetNode(nodeId);

            await Task.Yield();
            return MapNode(node);
        }

        public Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<IEmbeddingRelationTypeModel> relationTypes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SaveNodeAsync(Node source, Guid? requestId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private Node MapNode(INode node)
        {
            return MapNode(node, new List<Node>());
        }

        private Node MapNode(INode node, List<Node> mappedNodes)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == node.Id);
            if (m != null) return m;

            var nodeType = node.NodeType;
            Node result;
            if (nodeType.Kind == Kind.Attribute)
            {
                var value = AttributeType.ParseValue(node.Value, nodeType.AttributeType.ScalarType);
                var attributeType = new AttributeTypeWrapper(nodeType);
                result = new Attribute(node.Id, attributeType, node.Value, node.CreatedAt, node.UpdatedAt);
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

            foreach (var relatedNode in node.OutgoingRelations
                .Where(e => !e.Node.IsArchived && (e.Node.NodeType == null || !e.Node.NodeType.IsArchived)))
            {
                var mapped = MapNode(relatedNode.Node, mappedNodes);
                result.AddNode(mapped);
            }

            return result;
        }

        public Task<List<IncomingRelation>> GetIncomingEntities(IReadOnlyCollection<Guid> entityIds)
        {
            throw new NotImplementedException();
        }

        public Task<(IEnumerable<JObject> nodes, int count)> FilterNodeAsync(IEnumerable<string> typeNameList, ElasticFilter filter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
