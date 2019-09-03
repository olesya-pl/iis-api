﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

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
                    Value = ValueToString(attribute.Value, default)
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

        public async Task<IEnumerable<Node>> GetNodesByTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var derived = ontology.Where(e => e is EntityType && e.IsSubtypeOf(type)).Select(e => e.Id)
                .Concat(new[] { type.Id }).Distinct().ToArray();
            var ctxNodes = await _context.Nodes.Where(e => derived.Contains(e.TypeId) && !e.IsArchived)
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
                node.OutgoingRelations = groups.Single(e => e.Key == node.Id).ToArray();
            }

            var nodes = ctxNodes.Select(e => MapNode(e, ontology)).ToArray();

            return nodes;
        }
        
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            await semaphoreSlim.WaitAsync(cancellationToken);
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
                        .ToArrayAsync(cancellationToken);
                    ctxSource.OutgoingRelations = relations;
                }

                var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
                var node = MapNode(ctxSource, ontology);

                return node;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private Node MapNode(Context.Node ctxNode, IEnumerable<Type> ontology)
        {
            var type = ontology.First(e => e.Id == ctxNode.TypeId);
            Node node;
            if (type is AttributeType)
            {
                var attrType = (AttributeType)type;
                var value = ParseValue(ctxNode.Attribute.Value, attrType.ScalarTypeEnum);
                node = new Attribute(ctxNode.Id, (AttributeType)type, value);
            }
            else if (type is EntityType)
            {
                node = new Entity(ctxNode.Id, (EntityType)type);
            }
            else if (type is EmbeddingRelationType)
            {
                node = new Relation(ctxNode.Id, (EmbeddingRelationType)type);
                var target = MapNode(ctxNode.Relation.TargetNode, ontology);
                node.AddNode(target);
            }
            else throw new Exception("Unsupported.");

            foreach (var relatedNode in ctxNode.OutgoingRelations)
            {
                var mapped = MapNode(relatedNode.Node, ontology);
                node.AddNode(mapped);
            }

            return node;
        }

        private object ParseValue(string value, ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Boolean: return bool.Parse(value);
                case ScalarType.DateTime: return DateTime.Parse(value);
                case ScalarType.Decimal: return decimal.Parse(value);
                case ScalarType.Integer: return int.Parse(value);
                case ScalarType.String: return value;
                //case ScalarType.Json: return JObject.Parse(value);
                case ScalarType.Geo: return JObject.Parse(value);
                case ScalarType.File: return Guid.Parse(value);
                default: throw new NotImplementedException();
            }
        }

        private string ValueToString(object value, ScalarType scalarType)
        {
            return value.ToString();
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
