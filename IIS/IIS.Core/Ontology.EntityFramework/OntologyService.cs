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
            var sourceId = source.Id;
            var existing = await _context.Nodes.Where(e => e.Id == sourceId)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Node)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .FirstOrDefaultAsync(cancellationToken);

            foreach (var relationType in source.Type.RelatedTypes.OfType<EmbeddingRelationType>())
            {
                // Single attribute
                if (relationType.IsAttributeType && relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    var sourceRelation = source.Nodes.OfType<Relation>().SingleOrDefault(e => e.Type == relationType);
                    var existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Id == sourceRelation.Id);
                    ApplyChanges(existing, sourceRelation, existingRelation);
                }
                // Multiple attributes
                else if (relationType.IsAttributeType && relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                {
                    // source   existing
                    // 0        0   // nothing
                    // 1        1   // possible update
                    // 1        0   // new relation
                    // 0        1   // remove relation
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
                // Single Entity
                else if (relationType.IsEntityType && relationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    // todo: just recursion
                }
                // Multiple Entity
                else if (relationType.IsEntityType && relationType.EmbeddingOptions == EmbeddingOptions.Multiple)
                {
                    // MMMMMMULTIPLE RECURSION
                }
            }

            //// For each node from the input
            //foreach (var relation in source.Nodes.OfType<Relation>())
            //{
            //    var existingRelation = existing.OutgoingRelations.SingleOrDefault(e => e.Id == relation.Id);
            //    if (existingRelation != null)
            //    {
            //        // Possible relation update
            //        if (relation.IsAimedTo<Attribute>())
            //        {
            //            var attr = relation.Nodes.OfType<Attribute>().Single();
            //            // Reset value
            //            if (attr.Value is null)
            //            {
            //                existingRelation.Node.IsArchived = true;
            //                existingRelation.TargetNode.IsArchived = true;
            //            }
            //            // Change value
            //            else
            //            {
            //                // todo: Soft delete & create new relation
            //                //existingRelation.Node.IsArchived = true;
            //                //existingRelation.TargetNode.IsArchived = true;
            //                existingRelation.TargetNode.Attribute.Value = attr.Value.ToString();
            //            }
            //        }
            //        else if (relation.IsAimedTo<Entity>())
            //        {
            //            var entity = relation.Nodes.OfType<Entity>().Single();
            //            if (entity != null)
            //            {
            //                // handle next entity recursively
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // new relation
            //        // check diff in targets (if target exists -> link, otherwise -> create new target recursively)
            //        // todo: query for target??
            //        var ctxNode = Map(relation);
            //        existing.OutgoingRelations.Add(ctxNode.Relation);
            //    }
            //}

            //foreach (var relation in existing.OutgoingRelations)
            //{
            //    if (!source.Nodes.Any(e => e.Id == relation.Id))
            //    {
            //        // relation removed
            //        relation.Node.IsArchived = true;
            //        if (relation.TargetNode.Type.Kind == Kind.Attribute)
            //            relation.TargetNode.IsArchived = true;
            //        else; // ???
            //    }
            //}

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

        void ApplyChanges(Context.Node existing, Relation sourceRelation, Context.Relation existingRelation)
        {
            // Set null value
            if (sourceRelation is null && existingRelation != null)
            {
                Archive(existingRelation.Node);
                Archive(existingRelation.TargetNode);
            }
            // New relation
            else if (sourceRelation != null && existingRelation is null)
            {
                var relation = MapRelation(sourceRelation);
                existing.OutgoingRelations.Add(relation);
            }
            // Change value
            else
            {
                var existingValue = existingRelation.TargetNode.Attribute.Value;
                var sourceAttribute = (Attribute)sourceRelation.Target;
                var sourceValue = sourceAttribute.Value.ToString();
                if (existingValue != sourceValue)
                {
                    Archive(existingRelation.Node);
                    Archive(existingRelation.TargetNode);

                    var relation = MapRelation(sourceRelation);
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

        Context.Relation MapRelation(Relation relation)
        {
            var attribute = (Attribute)relation.Target;
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
                TargetNode = MapAttribute(attribute)
            };
        }

        void Archive(Context.Node node)
        {
            node.IsArchived = true;
            node.UpdatedAt = DateTime.UtcNow;
        }

        //private Context.Node Map(Node source)//, Node existingNode
        //{
        //    var node = new Context.Node
        //    {
        //        Id = source.Id,
        //        CreatedAt = source.CreatedAt,
        //        UpdatedAt = source.UpdatedAt,
        //        TypeId = source.Type.Id
        //    };

        //    if (source.Type is AttributeType)
        //    {
        //        var attribute = (Attribute)source;
        //        node.Attribute = new Context.Attribute { Id = source.Id, Value = attribute.Value.ToString(), Node = node };
        //        _context.Add(node.Attribute);
        //    }
        //    else if (source.Type is EmbeddingRelationType)
        //    {
        //        var relation = (Relation)source;
        //        ///
        //    }
        //    else if (source.Type is EntityType)
        //    {
        //        foreach (var childSrc in source.Nodes)
        //        {
        //            var childNode = Map(childSrc);
        //        }
        //    }

        //    _context.Add(node);

        //    return node;
        //}

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
                var type = ontology.Single(e => e.Name == ctxNode.Type.Name && e is RelationType) as RelationType;
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
