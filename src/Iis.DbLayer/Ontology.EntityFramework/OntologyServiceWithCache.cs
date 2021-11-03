using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Services.Contracts.Interfaces;
using Attribute = Iis.Domain.Attribute;
using Iis.Interfaces.AccessLevels;
using Iis.Domain.Users;
using Newtonsoft.Json.Linq;
using Iis.Domain.TreeResult;
using Iis.Interfaces.Ontology;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyServiceWithCache : IOntologyService
    {
        readonly IOntologyNodesData _data;
        readonly IElasticService _elasticService;
        readonly IElasticState _elasticState;
        public OntologyServiceWithCache(
            IOntologyNodesData data,
            IElasticService elasticService,
            IElasticState elasticState)
        {
            _data = data;
            _elasticService = elasticService;
            _elasticState = elasticState;
        }
        public IEnumerable<Node> GetEventsAssociatedWithEntity(Guid entityId)
        {
            IEnumerable<INode> events = GetEventsAssociatedWithEntityCore(entityId);

            return events.Select(ev => MapNode(ev)).AsEnumerable();
        }

        public Dictionary<Guid, int> CountEventsAssociatedWithEntities(HashSet<Guid> entityIds)
        {
            var res = new Dictionary<Guid, int>();

            foreach (var entityId in entityIds)
            {
                res.Add(entityId, GetEventsAssociatedWithEntityCore(entityId).Count());
            }

            return res;
        }

        private IEnumerable<INode> GetEventsAssociatedWithEntityCore(Guid entityId)
        {
            const string propertyName = "associatedWithEvent";
            var eventType = _data.Schema.GetEntityTypeByName(EntityTypeNames.Event.ToString());
            var property = eventType.GetRelationTypeByName(propertyName);
            if (property == null) throw new Exception($"Property does not exist: {EntityTypeNames.Event}.{propertyName}");

            var node = _data.GetNode(entityId);
            var events = node.IncomingRelations
                .Where(r => r.Node.NodeTypeId == property.Id)
                .Select(r => r.SourceNode);
            return events;
        }

        public IReadOnlyCollection<IncomingRelation> GetIncomingEntities(Guid entityId)
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

            return result;
        }

        public int GetRelationsCount(Guid entityId)
        {
            var node = _data.GetNode(entityId);
            if (node == null) return 0;

            return node.IncomingRelations.Count(r => r.RelationKind == RelationKind.Embedding)
                + node.OutgoingRelations.Count(r => r.RelationKind == RelationKind.Embedding && r.IsLinkToExternalObject);
        }

        public Dictionary<Guid, int> GetRelationsCount(HashSet<Guid> entityIds)
        {
            var res = new Dictionary<Guid, int>();

            foreach (var entityId in entityIds)
            {
                res.Add(entityId, GetRelationsCount(entityId));
            }

            return res;
        }

        public IReadOnlyCollection<Entity> GetEntitiesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            var nodes = _data.GetNodesByUniqueValue(nodeTypeId, value, valueTypeName);

            var result = nodes
                .Select(n => (Entity)MapNode(n))
                .ToList();

            return result;
        }

        public Node GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            var nodes = _data.GetNodesByUniqueValue(nodeTypeId, value, valueTypeName);

            var result = nodes
                .Select(n => MapNode(n))
                .FirstOrDefault();

            return result;
        }

        public IReadOnlyCollection<Guid> GetNodeIdListByFeatureIdList(IEnumerable<Guid> featureIdList)
        {
            return _data.Relations
                .Where(r => featureIdList.Contains(r.TargetNodeId))
                .Select(r => r.SourceNodeId)
                .Distinct()
                .ToArray();
        }

        public async Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<INodeTypeLinked> types, ElasticFilter filter, User user, CancellationToken cancellationToken = default)
        {
            var entitySearchGranted = _elasticService.ShouldReturnAllEntities(filter) ? user.IsEntityReadGranted() : user.IsEntitySearchGranted();
            var wikiSearchGranted = _elasticService.ShouldReturnAllEntities(filter) ? user.IsWikiReadGranted() : user.IsWikiSearchGranted();

            var derivedTypes = _data.Schema
                .GetNodeTypes(types.Select(t => t.Id))
                .Where(type => !type.IsAbstract 
                    && _elasticService.TypeIsAvalilable(type,
                        entitySearchGranted,
                        wikiSearchGranted));

            var isElasticSearch = !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));

            if (isElasticSearch)
            {
                var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(derivedTypes.Select(t => t.Name), filter, user.Id, cancellationToken);
                var nodes = _data.GetNodes(searchResult.Items.Select(e => e.Key));
                return nodes.Select(n => MapNode(n));
            }
            else
            {
                var nodes = GetNodesWithSuggestion(derivedTypes.Select(nt => nt.Id), filter);
                var filteredNodes = FilterAccessLevels(nodes, user);
                return filteredNodes.Select(n => MapNode(n));
            }
        }

        private IEnumerable<INode> FilterAccessLevels(IEnumerable<INode> nodes, User user)
        {
            var accessLevels = nodes
                .Where(p => string.Equals(p.NodeType.Name, nameof(AccessLevel), StringComparison.Ordinal))
                .Where(p => {
                    var stringValue = p.OutgoingRelations.FirstOrDefault(r => string.Equals(r.TypeName, "numericIndex", StringComparison.Ordinal)).TargetNode.Value;
                    _ = int.TryParse(stringValue, out var value);
                    return user.IsAdmin || value <= user.AccessLevel;
                })
                .OrderBy(n => {
                    var p = n.GetSingleProperty("numericIndex");
                    return p == null ? 0 : int.Parse(p.Value);
                });

            return accessLevels
                .Concat(nodes.Where(p => !string.Equals(p.NodeType.Name, nameof(AccessLevel), StringComparison.Ordinal)));
        }

        private List<INode> GetNodesWithSuggestion(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            return GetNodesWithSuggestionCore(derived, filter)
                .Skip(filter.Offset)
                .Take(filter.Limit)
                .ToList();
        }
        private IEnumerable<INode> GetNodesWithSuggestionCore(IEnumerable<Guid> derived, ElasticFilter filter)
        {
            return _data.GetNodesByTypeIds(derived)
                .Where(n => string.IsNullOrWhiteSpace(filter.Suggestion) ||
                    n.OutgoingRelations.Any(
                        r => r.TargetNode.Value != null &&
                        r.TargetNode.Value.Contains(filter.Suggestion, StringComparison.OrdinalIgnoreCase)));
        }
        public async Task<int> GetNodesCountAsync(IEnumerable<INodeTypeLinked> types, ElasticFilter filter, User user, CancellationToken cancellationToken = default)
        {
            var derivedTypes = _data.Schema.GetNodeTypes(types.Select(t => t.Id));

            var isElasticSearch = !string.IsNullOrEmpty(filter.Suggestion) && _elasticService.TypesAreSupported(derivedTypes.Select(nt => nt.Name));
            if (isElasticSearch)
            {
                return await _elasticService.CountByConfiguredFieldsAsync(derivedTypes.Select(t => t.Name), filter);
            }
            else
            {
                var nodes = GetNodesWithSuggestionCore(derivedTypes.Select(nt => nt.Id), filter);
                nodes = FilterAccessLevels(nodes, user);
                return nodes.Count();
            }
        }
        public (IEnumerable<Node> nodes, int count) GetNodesByIds(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default)
        {
            var nodes = _data.GetNodes(matchList);
            return (nodes.Select(n => MapNode(n)), nodes.Count);
        }
        public IReadOnlyList<IAttributeBase> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit)
        {
            IReadOnlyList<IAttributeBase> result = _data.Nodes
                .Where(n => string.Equals(n.NodeType.Name, valueTypeName, StringComparison.Ordinal) &&
                       n.Value != null && n.Value.Contains(value, StringComparison.OrdinalIgnoreCase) &&
                       n.IncomingRelations.Any(r => r.SourceNode.NodeTypeId == nodeTypeId))
                .Select(n => (IAttributeBase)(n.Attribute))
                .Take(limit)
                .ToList();

            return result;
        }
        public Node GetNode(Guid nodeId)
        {
            var node = _data.GetNode(nodeId);

            if(node is null) return null;

            return MapNode(node);
        }
        public IReadOnlyCollection<Node> LoadNodes(IEnumerable<Guid> nodeIds, IEnumerable<INodeTypeLinked> relationTypes)
        {
            var nodes = _data.GetNodes(nodeIds.Distinct());
            return nodes.Select(n => MapNode(n, relationTypes)).ToList();
        }

        public void RemoveNodeAndRelations(Node node)
        {
            _data.RemoveNodeAndRelations(node.Id);
        }

        public void RemoveNode(Node node)
        {
            _data.RemoveNode(node.Id);
        }

        public void SaveNode(Node source)
        {
            _data.WriteLock(() =>
            {
                var node = _data.GetNode(source.Id) ?? _data.CreateNode(source.Type.Id, source.Id);

                _data.SetNodeUpdatedAt(source.Id, source.UpdatedAt);

                SaveRelations(source, node);
            });
        }
        private void SaveRelations(Node source, INode existing)
        {
            foreach (INodeTypeLinked relationType in source.Type.AllProperties)
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
                _data.RemoveNodeAndRelations(existingRelation.Id);
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
                    _data.RemoveNodeAndRelations(existingRelation.Id);
                    CreateRelation(sourceRelation, existing.Id);
                }
            }
        }
        private IRelation CreateRelation(Relation relation, Guid sourceId)
        {
            if (relation.Target is Attribute)
            {
                var attribute = relation.Target as Attribute;
                var value = AttributeType.ValueToString(attribute.Value, attribute.Type.ScalarTypeEnum);
                return _data.CreateRelationWithAttribute(sourceId, relation.Type.Id, value);
            }
            else
            {
                return _data.CreateRelation(sourceId, relation.Target.Id, relation.Type.Id);
            }
        }
        private Node MapNode(INode node, IEnumerable<INodeTypeLinked> relationTypes = null)
        {
            return MapNode(node, new List<Node>(), relationTypes);
        }
        private Node MapNode(INode node, List<Node> mappedNodes, IEnumerable<INodeTypeLinked> relationTypes = null)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == node.Id);
            if (m != null) return m;

            var nodeType = node.NodeType;
            Node result;
            if (nodeType.Kind == Kind.Attribute)
            {
                var value = AttributeType.ParseValue(node.Value, nodeType.AttributeType.ScalarType);
                var attributeType = nodeType;
                result = new Attribute(node.Id, attributeType, value, node.CreatedAt, node.UpdatedAt);
            }
            else if (nodeType.Kind == Kind.Entity)
            {
                var entityType = nodeType;
                result = new Entity(node.Id, entityType, node.CreatedAt, node.UpdatedAt);
                mappedNodes.Add(result);
            }
            else if (nodeType.Kind == Kind.Relation)
            {
                var relationType = nodeType;
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
            result.OriginalNode = node;
            return result;
        }
        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeAsync(
            IEnumerable<string> typeNameList, 
            ElasticFilter filter,
            User user, 
            CancellationToken cancellationToken = default)
        {
            if(_elasticService.ShouldReturnNoEntities(filter)) return SearchEntitiesByConfiguredFieldsResult.Empty;

            var entitySearchGranted = _elasticService.ShouldReturnAllEntities(filter) ? user.IsEntityReadGranted() : user.IsEntitySearchGranted();
            var wikiSearchGranted = _elasticService.ShouldReturnAllEntities(filter) ? user.IsWikiReadGranted() : user.IsWikiSearchGranted();

            var derivedTypeNames = _data.Schema
                .GetEntityTypesByName(typeNameList, true)
                .Where(type => _elasticService.TypeIsAvalilable(type,
                        entitySearchGranted,
                        wikiSearchGranted))
                .Select(nt => nt.Name)
                .Distinct();
            var res = await _elasticService.SearchEntitiesByConfiguredFieldsAsync(derivedTypeNames, filter, user.Id);
            return RemoveDuplicatesInHighlights(res);
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(IEnumerable<string> typeNameList, 
            ElasticFilter filter, 
            CancellationToken cancellationToken = default)
        {
            var derivedTypeNames = _data.Schema
                .GetEntityTypesByName(typeNameList, true)
                .Select(nt => nt.Name)
                .Distinct();

            var isElasticSearch = _elasticService.TypesAreSupported(derivedTypeNames);

            if (isElasticSearch)
            {
                return await _elasticService.FilterNodeCoordinatesAsync(derivedTypeNames, filter);
            }
            else
            {
                return new SearchEntitiesByConfiguredFieldsResult();
            }
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> SearchEventsAsync(ElasticFilter filter, User user)
        {
            if (!string.IsNullOrEmpty(filter.SortColumn) && !string.IsNullOrEmpty(filter.SortOrder))
            {
                filter.SortColumn = GetSortColumnForElastic(filter.SortColumn);
                filter.SortOrder = filter.SortOrder;
            }

            var response = await _elasticService.SearchByConfiguredFieldsAsync(_elasticState.EventIndexes, filter, user.Id);

            return new SearchEntitiesByConfiguredFieldsResult()
            {
                Count = response.Count,
                Entities = response.Items.Select(x => x.Value.SearchResult).ToList()
            };
        }

        private static string GetSortColumnForElastic(string sortColumn)
        {
            return sortColumn switch
            {
                "name" => "name.keyword",
                "eventImportance" => "importance.code.keyword",
                "eventState" => "state.code.keyword",
                "startAt" => "startsAt",
                "updatedAt" => "UpdatedAt",
                _ => null
            };
        }

        public string GetAttributeValueByDotName(Guid id, string dotName)
        {
            var node = _data.GetNode(id);
            return node?.GetSingleProperty(dotName)?.Value;
        }

        private SearchEntitiesByConfiguredFieldsResult RemoveDuplicatesInHighlights(SearchEntitiesByConfiguredFieldsResult res)
        {
            const string HIGHLIGHT = "highlight";
            const string HISTORICAL = "historical";
            const string NODE_TYPE_NAME = "NodeTypeName";

            foreach (var jObj in res.Entities)
            {
                if (!jObj.ContainsKey(HIGHLIGHT) || jObj[HIGHLIGHT].Type == JTokenType.Null) continue;

                var nodeTypeName = jObj[NODE_TYPE_NAME].ToString();

                var highlight = (JObject)jObj[HIGHLIGHT];

                var names = new List<string>();
                foreach (var pair in highlight)
                {
                    names.Add(pair.Key);
                }

                foreach (var name in names)
                {
                    var clearName = name.StartsWith(HISTORICAL) ?
                        name.Substring(HISTORICAL.Length + 1) :
                        name;

                    var dotName = $"{nodeTypeName}.{clearName}";
                    var alias = _data.Schema.GetAlias(dotName);
                    if (alias != null && names.Contains(alias))
                        highlight.Remove(name);
                }
            }
            return res;
        }

        public TreeResultList GetEventTypes(string suggestion)
        {
            const string NAME = "name";
            const string CHILD = "child";

            var eventTypeType = _data.Schema.GetEntityTypeByName("EventType");
            if (eventTypeType == null) throw new Exception("EventType type is not found");

            var nodes = _data.GetNodesByTypeId(eventTypeType.Id)
                .Where(n => (string.IsNullOrEmpty(suggestion) ||
                    n.GetSingleDirectProperty(NAME)?.Value?.Contains(suggestion, StringComparison.OrdinalIgnoreCase) == true)
                    && !n.OutgoingRelations.Any(r => r.Node.NodeType.Name == CHILD))
                .OrderBy(n => n.GetSingleDirectProperty(NAME)?.Value);

            var result = new TreeResultList().Init(
                    nodes, 
                    n => n.GetSingleProperty(NAME)?.Value,
                    n => n.Id.ToString("N"),
                    n => n.IncomingRelations
                        .Where(r => r.Node.NodeType.Name == CHILD)
                        .Select(r => r.SourceNode)
                        .SingleOrDefault()
                        ?.GetSingleDirectProperty(NAME)
                        ?.Value
                );

            return result;
        }

        public IReadOnlyCollection<ObjectFeatureRelation> GetObjectFeatureRelationCollection(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return _data.GetNodes(nodeIdCollection)
                .SelectMany(n => n.OutgoingRelations.Where(r => r.TargetNode.NodeType.IsObjectSign))
                .Select(r => new ObjectFeatureRelation(r.SourceNodeId, r.TargetNodeId))
                .ToArray();
        }

        public IReadOnlyCollection<Guid> GetSignIds(Guid nodeId)
        {
            return _data.GetNode(nodeId)
                .GetDirectRelations()
                .Where(_ => _.Node.NodeType.Name == "sign")
                .Select(_ => _.TargetNodeId)
                .ToList();
        }
    }
}
