using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IIS.Core.Ontology.EntityFramework
{
    public class OntologyService : IOntologyService
    {
        private readonly OntologyContext _context;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IMemoryCache _cache;

        public OntologyService(OntologyContext context, IOntologyProvider ontologyProvider, IMemoryCache cache)
        {
            _context = context;
            _ontologyProvider = ontologyProvider;
            _cache = cache;
        }

        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            var existing = _context.Nodes.Local.FirstOrDefault(e => e.Id == source.Id);

            if (existing is null)
            {
                existing = new Context.Node
                {
                    Id = source.Id,
                    TypeId = source.Type.Id,
                    CreatedAt = source.CreatedAt,
                    UpdatedAt = source.UpdatedAt
                };
                _context.Add(existing);
            }

            foreach (var relationType in source.Type.AllProperties)
            {
                if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    var sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type == relationType);
                    var existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Node.TypeId == relationType.Id);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                else
                {
                    var sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type == relationType);
                    var existingRelations = existing.OutgoingRelations.Where(e => e.Node.TypeId == relationType.Id);
                    var pairs = sourceRelations.FullOuterJoin(existingRelations, e => e.Id, e => e.Id);
                    foreach (var pair in pairs)
                    {
                        ApplyChanges(existing, pair.Left, pair.Right);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        void ApplyChanges(Context.Node existing, Relation sourceRelation, Context.Relation existingRelation)
        {
            if (sourceRelation is null && existingRelation is null) return;

            // Set null value
            if (sourceRelation is null && existingRelation != null)
            {
                Archive(existingRelation.Node);
                //Archive(existingRelation.TargetNode);
            }
            // New relation
            else if (sourceRelation != null && existingRelation is null)
            {
                var relation = MapRelation(sourceRelation);
                existing.OutgoingRelations.Add(relation);
            }
            // Change target
            else
            {
                var existingId = existingRelation.TargetNode.Id;
                var sourceId = sourceRelation.Target.Id;
                if (existingId != sourceId)
                {
                    Archive(existingRelation.Node);

                    var relation = MapRelation(sourceRelation);
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

        Context.Node MapAttribute(Attribute attribute)
        {
            return new Context.Node
            {
                Id = attribute.Id,
                CreatedAt = attribute.CreatedAt,
                UpdatedAt = attribute.UpdatedAt,
                TypeId = attribute.Type.Id,
                Attribute = new Context.Attribute
                {
                    Id = attribute.Id,
                    Value = AttributeType.ValueToString(attribute.Value, default)
                }
            };
        }

        Context.Node MapEntity(Entity entity)
        {
            var existing = _context.Nodes.Local.Single(e => e.Id == entity.Id);
            return existing;
        }

        Context.Relation MapRelation(Relation relation)
        {
            var target = relation.Target is Attribute
                ? MapAttribute((Attribute)relation.Target)
                : MapEntity((Entity)relation.Target);
            return new Context.Relation
            {
                Id = relation.Id,
                Node = new Context.Node
                {
                    Id = relation.Id,
                    CreatedAt = relation.CreatedAt,
                    UpdatedAt = relation.UpdatedAt,
                    TypeId = relation.Type.Id
                },
                TargetNode = target
            };
        }

        void Archive(Context.Node node)
        {
            node.IsArchived = true;
            node.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<Type> types, NodeFilter filter, CancellationToken cancellationToken = default)
        {
            var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            var derived = types.SelectMany(e => ontology.GetChildTypes(e)).Select(e => e.Id)
                .Concat(types.Select(e => e.Id)).Distinct().ToArray();

            if (filter.Suggestion != null)
                return await GetNodesInternalWithSuggestionAsync(ontology, derived, filter.Limit, filter.Offset, filter.Suggestion, cancellationToken);
            if (filter.SearchCriteria.Count > 0)
                return await GetNodesInternalWithCriteriaAsync(ontology, derived, filter.Limit, filter.Offset,
                    filter.SearchCriteria, filter.AnyOfCriteria, cancellationToken);
            return await GetNodesInternalAsync(ontology, derived, filter.Limit, filter.Offset, cancellationToken);
        }

        private async Task<IEnumerable<Node>> GetNodesInternalAsync(Ontology ontology, Guid[] derived,
            int limit, int offset, CancellationToken cancellationToken = default)
        {
            var ctxNodes = await _context.Nodes.Where(e => derived.Contains(e.TypeId) && !e.IsArchived)
                .Skip(offset).Take(limit)
                .ToArrayAsync(cancellationToken);
            // prefetch +1 level
            var nodeIds = ctxNodes.Select(e => e.Id).ToArray();
            var relations = await _context.Relations.Where(e => nodeIds.Contains(e.SourceNodeId) && !e.Node.IsArchived)
                        .Include(e => e.Node)
                        .Include(e => e.TargetNode)
                        .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                        .ToArrayAsync(cancellationToken);

            var groups = relations.GroupBy(e => e.SourceNodeId);
            foreach (var node in ctxNodes)
            {
                node.OutgoingRelations = groups.Single(e => e.Key == node.Id).ToList();
            }

            var nodes = ctxNodes.Select(e => MapNode(e, ontology)).ToArray();

            return nodes;
        }

        private async Task<IEnumerable<Node>> GetNodesInternalWithSuggestionAsync(Ontology ontology, Guid[] derived,
            int limit, int offset, string suggestion, CancellationToken cancellationToken = default)
        {
            var relationsQ = _context.Relations
                .Include(e => e.Node)
                .Include(e => e.SourceNode)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => derived.Contains(e.SourceNode.TypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);
            if (suggestion != null)
                relationsQ = relationsQ.Where(e => e.TargetNode.Attribute.Value.Contains(suggestion));
            var ctxNodes = await relationsQ.Select(e => e.SourceNode).Distinct()
                .Skip(offset).Take(limit).ToArrayAsync(cancellationToken);

            return ctxNodes.Select(e => MapNode(e, ontology)).ToArray();
        }

        private async Task<IEnumerable<Node>> GetNodesInternalWithCriteriaAsync(Ontology ontology, Guid[] derived,
            int limit, int offset, List<Tuple<EmbeddingRelationType, string>> criteria, bool anyOfCriteria,
            CancellationToken cancellationToken = default)
        {
            var relationsQ = _context.Relations
//                .Include(e => e.Node)
                .Include(e => e.SourceNode)
//                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => derived.Contains(e.SourceNode.TypeId) && !e.Node.IsArchived && !e.SourceNode.IsArchived);

            var predicate = PredicateBuilder.New<Context.Relation>(false);
            foreach (var c in criteria)
            {
                var (relation, value) = c;
                if (relation.IsAttributeType)
                    predicate.Or(e => e.Node.TypeId == relation.Id && e.TargetNode.Attribute.Value.Contains(value));
                else
                    predicate.Or(e => e.Node.TypeId == relation.Id && e.TargetNodeId == Guid.Parse(value));
            }

            relationsQ = relationsQ.Where(predicate);

            Context.Node[] ctxNodes;
            if (anyOfCriteria)
            {
                ctxNodes = await relationsQ.Select(e => e.SourceNode).Distinct()
                    .Skip(offset).Take(limit)
                    .ToArrayAsync(cancellationToken);
            }
            else
            {
                var nodeIds = await relationsQ
                    .GroupBy(e => e.SourceNodeId)
                    .Where(g => g.Count() == criteria.Count)
                    .Select(g => g.Key).ToArrayAsync(cancellationToken);
                ctxNodes = await _context.Nodes.Where(e => nodeIds.Contains(e.Id))
                    .Skip(offset).Take(limit)
                    .ToArrayAsync(cancellationToken);
            }

            return ctxNodes.Select(e => MapNode(e, ontology)).ToArray();
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
                    var relations = new List<Context.Relation>();
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
                            var r = new Context.Relation
                            {
                                Id = rel.Id,
                                TargetNodeId = rel.SourceNodeId,
                                TargetNode = rel.SourceNode,
                                SourceNodeId = rel.TargetNodeId,
                                SourceNode = rel.TargetNode
                            };
                            r.Node = new Context.Node
                            {
                                Id = rel.Id,
                                TypeId = map[rel.Node.TypeId],
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

        private IQueryable<Context.Relation> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = _context.Relations
                .Include(e => e.Node)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.SourceNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.TypeId));
            return relationsQ;
        }

        private IQueryable<Context.Relation> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds)
        {
            var relationsQ = _context.Relations
                .Include(e => e.Node)
                .Include(e => e.SourceNode).ThenInclude(e => e.Attribute)
                .Where(e => nodeIds.Contains(e.TargetNodeId) && !e.Node.IsArchived);
            if (relationIds != null)
                relationsQ = relationsQ.Where(e => relationIds.Contains(e.Node.TypeId));
            return relationsQ;
        }

        private void FillRelations(List<Context.Node> nodes, List<Context.Relation> relations)
        {
            var nodesDict = nodes.ToDictionary(n => n.Id);
            foreach (var node in nodesDict.Values)
                node.OutgoingRelations = new List<Context.Relation>();
            foreach (var relation in relations)
                nodesDict[relation.SourceNodeId].OutgoingRelations.Add(relation);
        }


        private Node MapNode(Context.Node ctxNode, Ontology ontology)
        {
            return MapNode(ctxNode, ontology, new List<Node>());
        }

        private Node MapNode(Context.Node ctxNode, Ontology ontology, List<Node> mappedNodes)
        {
            var m = mappedNodes.SingleOrDefault(e => e.Id == ctxNode.Id);
            if (m != null) return m;

            var type = ontology.GetType(ctxNode.TypeId)
                       ?? throw new ArgumentException($"Ontology type with id {ctxNode.TypeId} was not found.");
            Node node;
            if (type is AttributeType attrType)
            {
                var value = AttributeType.ParseValue(ctxNode.Attribute.Value, attrType.ScalarTypeEnum);
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

            foreach (var relation in ctxNode.IncomingRelations)
                Archive(relation.Node);

            Archive(ctxNode);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
