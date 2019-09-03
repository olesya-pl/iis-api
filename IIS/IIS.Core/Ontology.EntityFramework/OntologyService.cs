using System;
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
            //return new Context.Node
            //{
            //    Id = entity.Id,
            //    CreatedAt = entity.CreatedAt,
            //    UpdatedAt = entity.UpdatedAt,
            //    TypeId = entity.Type.Id
            //};
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
            var derived = ontology.Where(e => e is EntityType && e.IsSubtypeOf(type)).Select(e => e.Id).ToArray();
            var ctxNodes = await _context.Nodes.Where(e => derived.Contains(e.Type.Id) && !e.IsArchived)
                .Include(n => n.Type)
                .ToArrayAsync(cancellationToken);

            var nodes = ctxNodes.Select(e => MapNode(e, ontology)).ToArray();

            return nodes;
        }

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            var ctxSource = await _context.Nodes.Where(e => e.Id == nodeId && !e.IsArchived)
                .Include(e => e.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node.Type.RelationType)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode.Type.AttributeType)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .FirstOrDefaultAsync(cancellationToken);

            // todo: fix
            //var relationIds = toLoad.Select(e => e.Id);
            //var relations = await _context.Relations.Where(e => e.SourceNode.Id == sourceId && relationIds.Contains(e.Node.Type.Id))
            //    .Include(e => e.SourceNode)
            //    .ToArrayAsync(cancellationToken);
            //var ctxSource = relations.First().SourceNode;
            //source = MapNode(ctxSource);

            if (ctxSource is null) return null;

            ctxSource.OutgoingRelations = ctxSource.OutgoingRelations.Where(r => !r.Node.IsArchived).ToList(); // no way to filter inner entities with EF

            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var node = MapNode(ctxSource, ontology);

            return node;
        }

        private Node MapNode(Context.Node ctxNode, IEnumerable<Type> ontology)
        {
            //var type = ontology.First(e => e.Id == ctxNode.TypeId);
            Node node;
            if (ctxNode.Type.Kind == Kind.Attribute)
            {
                var type = ontology.Single(e => e.Name == ctxNode.Type.Name && e is AttributeType) as AttributeType;
                var value = ParseValue(ctxNode.Attribute.Value, ctxNode.Type.AttributeType.ScalarType);
                node = new Attribute(ctxNode.Id, type, value);
            }
            else if (ctxNode.Type.Kind == Kind.Entity)
            {
                var type = ontology.Single(e => e.Name == ctxNode.Type.Name && e is EntityType) as EntityType;
                node = new Entity(ctxNode.Id, type);
            }
            else if (ctxNode.Type.Kind == Kind.Relation && ctxNode.Type.RelationType.Kind == RelationKind.Embedding)
            {
                var type = ontology.Single(e => e.Id == ctxNode.TypeId && e is RelationType) as RelationType;
                node = new Relation(ctxNode.Id, type);
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

        private object ParseValue(string value, Context.ScalarType scalarType)
        {
            switch (scalarType)
            {
                case Context.ScalarType.Boolean: return bool.Parse(value);
                case Context.ScalarType.Date: return DateTime.Parse(value);
                case Context.ScalarType.Decimal: return decimal.Parse(value);
                case Context.ScalarType.Int: return int.Parse(value);
                case Context.ScalarType.String: return value;
                case Context.ScalarType.Json: return JObject.Parse(value);
                case Context.ScalarType.Geo: return JObject.Parse(value);
                case Context.ScalarType.File: return Guid.Parse(value);
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
