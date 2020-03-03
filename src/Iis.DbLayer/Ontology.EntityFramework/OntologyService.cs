using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Utility;
using IIS.Domain;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Attribute = Iis.Domain.Attribute;
using EmbeddingOptions = Iis.Domain.EmbeddingOptions;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyService : IOntologyService
    {
        private readonly OntologyContext _context;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IMemoryCache _cache;
        private readonly IElasticService _elasticService;

        public OntologyService(OntologyContext context, IOntologyProvider ontologyProvider, IMemoryCache cache, IElasticService elasticService)
        {
            _context = context;
            _ontologyProvider = ontologyProvider;
            _cache = cache;
            _elasticService = elasticService;
        }

        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            NodeEntity existing = _context.Nodes.Local.FirstOrDefault(e => e.Id == source.Id);

            if (existing is null)
            {
                existing = new NodeEntity
                {
                    Id = source.Id,
                    NodeTypeId = source.Type.Id,
                    CreatedAt = source.CreatedAt,
                    UpdatedAt = source.UpdatedAt
                };
                _context.Nodes.Add(existing);
            }

            foreach (EmbeddingRelationType relationType in source.Type.AllProperties)
            {
                if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    Relation sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type == relationType);
                    RelationEntity existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Node.NodeTypeId == relationType.Id);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                else
                {
                    IEnumerable<Relation> sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type == relationType);
                    IEnumerable<RelationEntity> existingRelations = existing.OutgoingRelations.Where(e => e.Node.NodeTypeId == relationType.Id);
                    var pairs = sourceRelations.FullOuterJoin(existingRelations, e => e.Id, e => e.Id);
                    foreach (var pair in pairs)
                    {
                        ApplyChanges(existing, pair.Left, pair.Right);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await _elasticService.PutNodeAsync(source.Id);
        }

        public async Task SaveNodesAsync(IEnumerable<Node> nodes, CancellationToken cancellationToken = default)
        {
            IQueryable<NodeEntity> query =
                from node in _context.Nodes
                    .Include(x => x.Relation)
                    .Include(x => x.Attribute)
                    .Include(x => x.OutgoingRelations)
                        .ThenInclude(x => x.Node)
                select node;
            Dictionary<Guid, NodeEntity> existingNodes = await query.ToDictionaryAsync(x => x.Id, cancellationToken);

            foreach (Node source in nodes)
            {
                if (!existingNodes.TryGetValue(source.Id, out NodeEntity existing))
                {
                    existing = new NodeEntity
                    {
                        Id = source.Id,
                        NodeTypeId = source.Type.Id,
                        CreatedAt = source.CreatedAt,
                        UpdatedAt = source.UpdatedAt
                    };
                    _context.Nodes.Add(existing);
                    existingNodes.Add(existing.Id, existing);
                }

                foreach (EmbeddingRelationType relationType in source.Type.AllProperties)
                {
                    if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                    {
                        Relation sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type == relationType);
                        RelationEntity existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Node.NodeTypeId == relationType.Id);
                        ApplyChanges(existing, sourceRelation, existingRelation);
                    }
                    else
                    {
                        IEnumerable<Relation> sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type == relationType);
                        IEnumerable<RelationEntity> existingRelations = existing.OutgoingRelations.Where(e => e.Node.NodeTypeId == relationType.Id);
                        var pairs = sourceRelations.FullOuterJoin(existingRelations, e => e.Id, e => e.Id);
                        foreach (var pair in pairs)
                        {
                            ApplyChanges(existing, pair.Left, pair.Right);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            foreach (Node source in nodes)
            {
                await _elasticService.PutNodeAsync(source.Id);
            }
        }

        void ApplyChanges(NodeEntity existing, Relation sourceRelation, RelationEntity existingRelation)
        {
            if (sourceRelation == null && existingRelation == null)
                return;

            // Set null value
            if (sourceRelation == null && existingRelation != null)
            {
                Archive(existingRelation.Node);
                //Archive(existingRelation.TargetNode);
            }
            // New relation
            else if (sourceRelation != null && existingRelation == null)
            {
                var relation = MapRelation(sourceRelation);
                existing.OutgoingRelations.Add(relation);
            }
            // Change target
            else
            {
                Guid existingId = existingRelation.TargetNode.Id;
                Guid sourceId = sourceRelation.Target.Id;
                if (existingId != sourceId)
                {
                    Archive(existingRelation.Node);

                    RelationEntity relation = MapRelation(sourceRelation);
                    relation.Id = Guid.NewGuid();
                    relation.Node.Id = relation.Id;
                    // set tracked target
                    if (sourceRelation.Target is Attribute)
                    {
                        //
                    }
                    else
                    {
                        relation.TargetNode = null;
                        relation.TargetNodeId = sourceId;
                    }
                    existing.OutgoingRelations.Add(relation);

                    sourceRelation.Id = relation.Id;
                    sourceRelation.CreatedAt = DateTime.UtcNow;
                    sourceRelation.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        NodeEntity MapAttribute(Attribute attribute)
        {
            return new NodeEntity
            {
                Id = attribute.Id,
                CreatedAt = attribute.CreatedAt,
                UpdatedAt = attribute.UpdatedAt,
                NodeTypeId = attribute.Type.Id,
                Attribute = new AttributeEntity
                {
                    Id = attribute.Id,
                    Value = AttributeType.ValueToString(attribute.Value, ((AttributeType)attribute.Type).ScalarTypeEnum)
                }
            };
        }

        NodeEntity MapEntity(Entity entity)
        {
            return _context.Nodes.Local.SingleOrDefault(e => e.Id == entity.Id)
                   ?? _context.Nodes.Single(e => e.Id == entity.Id);

        }

        RelationEntity MapRelation(Relation relation)
        {
            var target = relation.Target is Attribute
                ? MapAttribute((Attribute)relation.Target)
                : MapEntity((Entity)relation.Target);
            return new RelationEntity
            {
                Id = relation.Id,
                Node = new NodeEntity
                {
                    Id = relation.Id,
                    CreatedAt = relation.CreatedAt,
                    UpdatedAt = relation.UpdatedAt,
                    NodeTypeId = relation.Type.Id
                },
                TargetNode = target
            };
        }

        void Archive(NodeEntity node)
        {
            node.IsArchived = true;
            node.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<int> GetNodesCountAsync(IEnumerable<NodeType> types, NodeFilter filter, CancellationToken cancellationToken = default)
        {
            var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            await _context.Semaphore.WaitAsync(cancellationToken);
            try
            {
                var q = await GetNodesQueryAsync(ontology, types, filter, cancellationToken);
                return q.Distinct().Count();
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<NodeType> types, NodeFilter filter, CancellationToken cancellationToken = default)
        {
            var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            NodeEntity[] ctxNodes;
            await _context.Semaphore.WaitAsync(cancellationToken);
            try
            {
                var relationsQ = await GetNodesQueryAsync(ontology, types, filter, cancellationToken);
                // workaround because of wrong generated query with distinct and order by
                var workaroundQ = from a in relationsQ.Distinct()
                                  select new { Node = a, dummy = string.Empty };

                ctxNodes = await workaroundQ
                    .Skip(filter.Offset)
                    .Take(filter.Limit)
                    .Select(r => r.Node)
                    .ToArrayAsync(cancellationToken);
            }
            finally
            {
                _context.Semaphore.Release();
            }
            return ctxNodes.Select(e => MapNode(e, ontology)).ToArray();
        }

        private async Task<IQueryable<NodeEntity>> GetNodesQueryAsync(OntologyModel ontology, IEnumerable<NodeType> types, NodeFilter filter, CancellationToken cancellationToken = default)
        {
            var derivedTypes = types.SelectMany(e => ontology.GetChildTypes(e))
                .Concat(types).Distinct().ToArray();

            if (filter.SearchCriteria.Count > 0)
            {
                var result = await GetNodesInternalWithCriteriaAsync(derivedTypes.Select(t => t.Id), filter.SearchCriteria, filter.AnyOfCriteria,
                    filter.ExactMatch, cancellationToken);
                if (filter.Suggestion == null)
                    return result;
                var suggestedIds = (await GetNodesWithSuggestion(derivedTypes, filter.Suggestion, cancellationToken)).Select(n => n.Id).ToArray();
                return result.Where(n => suggestedIds.Contains(n.Id));
            }
            if (filter.Suggestion != null)
                return await GetNodesWithSuggestion(derivedTypes, filter.Suggestion, cancellationToken);
            return GetNodesInternal(derivedTypes.Select(t => t.Id));
        }

        private IQueryable<NodeEntity> GetNodesInternal(IEnumerable<Guid> derived)
        {
            return _context.Nodes.Where(e => derived.Contains(e.NodeTypeId) && !e.IsArchived);
        }

        private async Task<IQueryable<NodeEntity>> GetNodesWithSuggestion(IEnumerable<NodeType> types, string suggestion, CancellationToken cancellationToken = default)
        {
            if (_elasticService.TypesAreSupported(types.Select(nt => nt.Name)))
            {
                return await GetNodesByElasticAllFields(types, suggestion, cancellationToken);
            }
            else
            {
                return GetNodesInternalWithSuggestion(types.Select(nt => nt.Id), suggestion);
            }
        }

        private IQueryable<NodeEntity> GetNodesInternalWithSuggestion(IEnumerable<Guid> derived, string suggestion)
        {
            var relationsQ = _context.Relations
                .Include(e => e.SourceNode)
                .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);
            if (suggestion != null)
                relationsQ = relationsQ.Where(e =>
                    EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{suggestion}%"));
            return relationsQ.Select(e => e.SourceNode);
        }

        private async Task<IQueryable<NodeEntity>> GetNodesByElasticAllFields(IEnumerable<NodeType> types, string suggestion, CancellationToken cancellationToken = default)
        {
            var ids = await _elasticService.SearchByAllFieldsAsync(types.Select(t => t.Name), suggestion);
            return _context.Nodes.Where(node => !node.IsArchived && ids.Contains(node.Id));
        }

        private async Task<IQueryable<NodeEntity>> GetNodesInternalWithCriteriaAsync(IEnumerable<Guid> derived,
            List<Tuple<EmbeddingRelationType, string>> criteria, bool anyOfCriteria, bool exactMatch,
            CancellationToken cancellationToken = default)
        {
            var relationsQ = _context.Relations
                .Include(e => e.SourceNode)
                .Where(e => derived.Contains(e.SourceNode.NodeTypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);

            var predicate = PredicateBuilder.New<RelationEntity>(false);
            foreach (var c in criteria)
            {
                var (relation, value) = c;
                if (relation.IsEntityType)
                    predicate.Or(e => e.Node.NodeTypeId == relation.Id && e.TargetNodeId == Guid.Parse(value));
                else if (exactMatch)
                    predicate.Or(e => e.Node.NodeTypeId == relation.Id && e.TargetNode.Attribute.Value == value);
                else
                    predicate.Or(e => e.Node.NodeTypeId == relation.Id
                                      && EF.Functions.ILike(e.TargetNode.Attribute.Value, $"%{value}%"));
            }

            relationsQ = relationsQ.Where(predicate);

            NodeEntity[] ctxNodes;
            if (anyOfCriteria)
                return relationsQ.Select(e => e.SourceNode).Distinct();

            var nodeIds = await relationsQ
                .GroupBy(e => e.SourceNodeId)
                .Where(g => g.Count() == criteria.Count)
                .Select(g => g.Key).ToArrayAsync(cancellationToken);
            return _context.Nodes.Where(e => nodeIds.Contains(e.Id));
        }

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            await _context.Semaphore.WaitAsync(cancellationToken);
            try
            {
                var ctxSource = await _context.Nodes.FindAsync(nodeId);

                if (ctxSource is null) return null;
                if (!ctxSource.OutgoingRelations.Any())
                {
                    var relations = await _context.Relations.Where(e => e.SourceNodeId == nodeId && !e.Node.IsArchived)
                        .Include(e => e.Node)
                        .Include(e => e.TargetNode)
                        .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                        .ToListAsync(cancellationToken);
                    ctxSource.OutgoingRelations = relations;
                }

                var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
                var node = MapNode(ctxSource, ontology);

                return node;
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds,
            IEnumerable<EmbeddingRelationType> relationTypes, CancellationToken cancellationToken = default)
        {
            await _context.Semaphore.WaitAsync(cancellationToken);
            try
            {
                var nodes = await _context.Nodes.Where(e => nodeIds.Contains(e.Id)).ToListAsync(cancellationToken);

                if (relationTypes == null)
                {
                    var relations = await GetDirectRelationsQuery(nodeIds, null).ToListAsync(cancellationToken);
                    FillRelations(nodes, relations);
                }
                else
                {
                    var directIds = relationTypes.Where(r => !r.IsInversed).Select(r => r.Id).ToArray();
                    var inversedIds = relationTypes.Where(r => r.IsInversed).Select(r => r.DirectRelationType.Id).ToArray();
                    var relations = new List<RelationEntity>();
                    if (directIds.Length > 0)
                    {
                        var result = await GetDirectRelationsQuery(nodeIds, directIds).ToListAsync(cancellationToken);
                        relations.AddRange(result);
                    }

                    if (inversedIds.Length > 0)
                    {
                        var result = await GetInversedRelationsQuery(nodeIds, inversedIds).ToListAsync(cancellationToken);
                        var map = relationTypes.Where(r => r.IsInversed).ToDictionary(r => r.DirectRelationType.Id, r => r.Id);
                        foreach (var rel in result)
                        {
                            var r = new RelationEntity
                            {
                                Id = rel.Id,
                                TargetNodeId = rel.SourceNodeId,
                                TargetNode = rel.SourceNode,
                                SourceNodeId = rel.TargetNodeId,
                                SourceNode = rel.TargetNode
                            };
                            r.Node = new NodeEntity
                            {
                                Id = rel.Id,
                                NodeTypeId = map[rel.Node.NodeTypeId],
                                Relation = r
                            };
                            relations.Add(r);
                        }
                    }
                    FillRelations(nodes, relations);
                }

                var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
                return nodes.Select(n => MapNode(n, ontology)).ToList();
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        private IQueryable<RelationEntity> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = _context.Relations
                .Include(e => e.Node)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.SourceNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ;
        }

        private IQueryable<RelationEntity> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = _context.Relations
                .Include(e => e.Node)
                .Include(e => e.SourceNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.TargetNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.NodeTypeId));
            return relationsQ;
        }

        private void FillRelations(List<NodeEntity> nodes, List<RelationEntity> relations)
        {
            var nodesDict = nodes.ToDictionary(n => n.Id);
            foreach (var node in nodesDict.Values)
                node.OutgoingRelations = new List<RelationEntity>();
            foreach (var relation in relations)
                nodesDict[relation.SourceNodeId].OutgoingRelations.Add(relation);
        }


        private Node MapNode(NodeEntity ctxNode, OntologyModel ontology)
        {
            return MapNode(ctxNode, ontology, new List<Node>());
        }

        private Node MapNode(NodeEntity ctxNode, OntologyModel ontology, List<Node> mappedNodes)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == ctxNode.Id);
            if (m != null) return m;

            var type = ontology.GetType(ctxNode.NodeTypeId)
                       ?? throw new ArgumentException($"Ontology type with id {ctxNode.NodeTypeId} was not found.");
            Node node;
            if (type is AttributeType attrType)
            {
                var attribute = ctxNode.Attribute ?? _context.Attributes.SingleOrDefault(attr => attr.Id == ctxNode.Id);
                if (attribute == null)
                {
                    throw new Exception($"Attribute does not exists for attribute type node id = {ctxNode.Id}");
                }
                var value = AttributeType.ParseValue(attribute.Value, attrType.ScalarTypeEnum);
                node = new Attribute(ctxNode.Id, attrType, value, ctxNode.CreatedAt, ctxNode.UpdatedAt);
            }
            else if (type is EntityType entityType)
            {
                node = new Entity(ctxNode.Id, entityType, ctxNode.CreatedAt, ctxNode.UpdatedAt);
                mappedNodes.Add(node);
            }
            else if (type is EmbeddingRelationType relationType)
            {
                node = new Relation(ctxNode.Id, relationType, ctxNode.CreatedAt, ctxNode.UpdatedAt);
                var target = MapNode(ctxNode.Relation.TargetNode, ontology, mappedNodes);
                node.AddNode(target);
            }
            else throw new Exception($"Node mapping does not support ontology type {type.GetType()}.");

            foreach (var relatedNode in ctxNode.OutgoingRelations.Where(e => !e.Node.IsArchived))
            {
                var mapped = MapNode(relatedNode.Node, ontology, mappedNodes);
                node.AddNode(mapped);
            }

            return node;
        }

        public async Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default)
        {
            var ctxNode = _context.Nodes.Local.Single(n => n.Id == node.Id);

            var relations = _context.Relations
                .Include(r => r.Node)
                .Where(r => !r.Node.IsArchived && (r.TargetNodeId == node.Id || r.SourceNodeId == node.Id));

            foreach (var relation in relations)
                Archive(relation.Node);

            Archive(ctxNode);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}