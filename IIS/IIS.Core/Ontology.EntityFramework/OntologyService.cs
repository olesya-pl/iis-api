using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Ontology.EntityFramework
{
    public class OntologyService : IOntologyService
    {
        private readonly OntologyContext _context;
        private readonly IOntologyProvider _ontologyProvider;

        public OntologyService(OntologyContext context, IOntologyProvider ontologyProvider)
        {
            _context = context;
            _ontologyProvider = ontologyProvider;
        }

        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Nodes.Where(e => e.Id == source.Id)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .FirstOrDefaultAsync(cancellationToken);

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
                // Single attribute
                if (relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    var sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type == relationType);
                    var existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Id == sourceRelation?.Id);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                // Multiple attributes
                //relationType.IsAttributeType && 
                else if (relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                {
                    var sourceRelations = source.Nodes.OfType<Relation>().Where(e => e.Type == relationType);
                    var existingRelations = existing.OutgoingRelations.Where(e => e.Node.TypeId == relationType.Id);
                    var left =
                        from sourceRelation in sourceRelations
                        join existingRelation in existingRelations on sourceRelation.Id equals existingRelation.Id into se
                        from p in se.DefaultIfEmpty()
                        select new { Source = sourceRelation, Existing = p };
                    var right =
                        from existingRelation in existingRelations
                        join sourceRelation in sourceRelations on existingRelation.Id equals sourceRelation.Id into se
                        from p in se.DefaultIfEmpty()
                        select new { Source = p, Existing = existingRelation };
                    var union = left.Union(right);
                    foreach (var pair in union)
                    {
                        ApplyChanges(existing, pair.Source, pair.Existing);
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
                    existing.OutgoingRelations.Add(relation);
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
                    Value = attribute.Value.ToString()
                }
            };
        }

        Context.Node MapEntity(Entity entity)
        {
            //var existing = _context.Nodes.Single(e => e.Id == entity.Id);
            //return existing;
            //_context.Attach
            return new Context.Node
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                TypeId = entity.Type.Id
            };
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
            var ctxNodes = await _context.Nodes.Where(e => e.Type.Name == type.Name) // todo: replace with id search
                .Include(n => n.Type)
                .ToArrayAsync(cancellationToken);

            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var nodes = ctxNodes.Select(e => MapNode(e, ontology)).ToArray();

            return nodes;
        }

        public async Task<Node> LoadNodesAsync(Guid nodeId, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            var ctxSource = await _context.Nodes.Where(e => e.Id == nodeId)
                .Include(e => e.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node.Type.RelationType)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode.Type)
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

            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var node = MapNode(ctxSource, ontology);

            return node;
        }

        public async Task<IDictionary<string, IEnumerable<Node>>> GetNodesByTypesAsync(IEnumerable<string> typeNames,
            CancellationToken cancellationToken = default)
        {
            var ctxTypes = await _context.Types.Where(e => typeNames.Contains(e.Name))
                .Include(e => e.Nodes)
                .ToArrayAsync(cancellationToken);

            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var types = ctxTypes.ToDictionary(e => e.Name, e => e.Nodes.Select(ee => MapNode(ee, ontology)));

            return types;
        }

        public async Task<IDictionary<Guid, Node>> LoadNodesAsync(IEnumerable<Guid> sourceIds,
            IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            var ctxSources = await _context.Nodes.Where(e => sourceIds.Contains(e.Id))
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .ToArrayAsync(cancellationToken);

            var ontology = await _ontologyProvider.GetTypesAsync(cancellationToken);
            var sources = ctxSources.Select(e => MapNode(e, ontology)).ToDictionary(e => e.Id);

            return sources;
        }

        private Node MapNode(Context.Node ctxNode, IEnumerable<Type> ontology)
        {
            Node node;
            if (ctxNode.Type.Kind == Kind.Attribute)
            {
                var type = ontology.Single(e => e.Name == ctxNode.Type.Name && e is AttributeType) as AttributeType;
                node = new Attribute(ctxNode.Id, type, ctxNode.Attribute.Value);
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

        public async Task RemoveNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
