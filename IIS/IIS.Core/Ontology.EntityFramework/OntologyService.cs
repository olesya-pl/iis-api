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

        public OntologyService(OntologyContext context)
        {
            _context = context;
        }

        // if single attr:
        //      if new value -> replace value
        //      if no relation -> mark relation & attribute as deleted
        // if multiple attr:
        //      if

        public async Task SaveNodeAsync(Node source, CancellationToken cancellationToken = default)
        {
            var sourceId = source.Id;
            var ctxSource = await _context.Nodes.Where(e => e.Id == sourceId)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .FirstOrDefaultAsync(cancellationToken);

            foreach (var relation in source.Nodes)
            {
                var existingRelation = ctxSource.OutgoingRelations.SingleOrDefault(e => e.Id == relation.Id);
                if (existingRelation != null)
                {
                    // possible relation update
                    // check diff in targets
                    var attr = relation.Nodes.OfType<Attribute>().FirstOrDefault();
                    if (attr != null)
                    {
                        if (attr.Value is null)
                        {
                            // remove relation & attr
                            existingRelation.Node.IsArchived = true;
                            existingRelation.TargetNode.IsArchived = true;
                        }
                        else
                        {
                            // change value
                            existingRelation.TargetNode.Attribute.Value = attr.Value.ToString();
                        }
                    }
                    var entity = relation.Nodes.OfType<Entity>().FirstOrDefault();
                    if (entity != null)
                    {
                        // handle next entity recursively
                    }
                }
                else
                {
                    // new relation
                    // check diff in targets (if target exists -> link, otherwise -> create new target recursively)
                    // todo: query for target??
                    var ctxNode = Map(relation);
                    ctxSource.OutgoingRelations.Add(ctxNode.Relation);
                }
            }

            foreach (var relation in ctxSource.OutgoingRelations)
            {
                if (!source.Nodes.Any(e => e.Id == relation.Id))
                {
                    // relation removed
                    relation.Node.IsArchived = true;
                    if (relation.TargetNode.Type.Kind == Kind.Attribute)
                        relation.TargetNode.IsArchived = true;
                    else; // ???
                }
            }

            //var srcEntity = (Entity)source;
            //foreach (var node in srcEntity.Nodes)
            //{
            //    var relation = (Relation)node;
            //    var type = (EmbeddingRelationType)relation.Type;
            //    if (type.IsAttributeType)
            //    {
            //        var attrTarget = relation.Nodes.OfType<Attribute>().Single();
            //        var attrTargetType = (AttributeType)attrTarget.Type;

            //    }
            //    else if (type.IsEntityType)
            //    {

            //    }
            //}

            //var ctxNode = Map(source);

            //_context.Add(ctxNode);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private Context.Node Map(Node source)//, Node existingNode
        {
            var node = new Context.Node
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                TypeId = source.Type.Id
            };

            if (source.Type is AttributeType)
            {
                var attribute = (Attribute)source;
                node.Attribute = new Context.Attribute { Id = source.Id, Value = attribute.Value.ToString(), Node = node };
                _context.Add(node.Attribute);
            }
            else if (source.Type is EmbeddingRelationType)
            {
                var relation = (Relation)source;
                ///
            }
            else if (source.Type is EntityType)
            {
                foreach (var childSrc in source.Nodes)
                {
                    var childNode = Map(childSrc);
                }
            }

            _context.Add(node);

            return node;
        }

        public async Task<IEnumerable<Node>> GetNodesByTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            var typeId = type.Id;
            var ctxType = await _context.Types.Where(e => e.Name == type.Name) // todo: replace with id search
                .Include(e => e.Nodes)
                    .ThenInclude(n => n.Type)
                .FirstOrDefaultAsync(cancellationToken);

            if (ctxType is null) throw new Exception("The given type is not found.");

            var nodes = ctxType.Nodes.Select(MapNode).ToArray();

            return nodes;
        }

        public async Task<IDictionary<string, IEnumerable<Node>>> GetNodesByTypesAsync(IEnumerable<string> typeNames,
            CancellationToken cancellationToken = default)
        {
            var ctxTypes = await _context.Types.Where(e => typeNames.Contains(e.Name))
                .Include(e => e.Nodes)
                .ToArrayAsync(cancellationToken);

            var types = ctxTypes.ToDictionary(e => e.Name, e => e.Nodes.Select(MapNode));

            return types;
        }

        public async Task<Node> LoadNodesAsync(Node source, IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            var sourceId = source.Id;
            var ctxSource = await _context.Nodes.Where(e => e.Id == sourceId)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .FirstOrDefaultAsync(cancellationToken);

            // todo: fix
            //var relationIds = toLoad.Select(e => e.Id);
            //var relations = await _context.Relations.Where(e => e.SourceNode.Id == sourceId && relationIds.Contains(e.Node.Type.Id))
            //    .Include(e => e.SourceNode)
            //    .ToArrayAsync(cancellationToken);
            //var ctxSource = relations.First().SourceNode;
            //source = MapNode(ctxSource);

            source = MapNode(ctxSource);

            return source;
        }

        public async Task<IDictionary<Guid, Node>> LoadNodesAsync(IEnumerable<Guid> sourceIds,
            IEnumerable<RelationType> toLoad, CancellationToken cancellationToken = default)
        {
            var ctxSources = await _context.Nodes.Where(e => sourceIds.Contains(e.Id))
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .ToArrayAsync(cancellationToken);

            var sources = ctxSources.Select(MapNode).ToDictionary(e => e.Id);

            return sources;
        }

        private Node MapNode(Context.Node ctxNode)
        {
            if (ctxNode == null) return null;
            Node node;
            if (ctxNode.Type.Kind == Kind.Attribute)
                node = new Attribute(ctxNode.Id, ctxNode.Attribute.Value);
            else if (ctxNode.Type.Kind == Kind.Entity)
                node = new Entity(ctxNode.Id);
            else if (ctxNode.Type.Kind == Kind.Relation && ctxNode.Type.RelationType.Kind == RelationKind.Embedding)
                node = new Relation(ctxNode.Id);
            else throw new Exception("Unsupported.");

            foreach (var relatedNode in ctxNode.OutgoingRelations)
            {
                var mapped = MapNode(relatedNode.Node);
                node.AddNode(mapped);
            }

            return node;
        }

        public async Task SaveTypeAsync(Type type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveTypeAsync(string typeName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
