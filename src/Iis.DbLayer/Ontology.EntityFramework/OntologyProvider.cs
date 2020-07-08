using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using IIS.Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyProvider : IOntologyProvider
    {
        private readonly OntologyContext _context;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private OntologyModel _ontology;

        public OntologyProvider(OntologyContext contextFactory)
        {
            _context = contextFactory;
        }

        public IOntologyModel GetOntology()
        {
            _locker.EnterReadLock();
            try
            {
                // Try to hit in cache
                if (_ontology != null) return _ontology;
            }
            finally
            {
                _locker.ExitReadLock();
            }

            _locker.EnterWriteLock();
            try
            {
                // Double check
                if (_ontology != null) return _ontology;

                // Query primary source and update the cache
                var types = _context.NodeTypes.Where(e => !e.IsArchived && e.Kind != Kind.Relation)
                    .Include(e => e.IncomingRelations).ThenInclude(e => e.NodeType)
                    .Include(e => e.OutgoingRelations).ThenInclude(e => e.NodeType)
                    .Include(e => e.AttributeType)
                    .ToArray();
                var result = types.Select(e => MapType(e)).ToList();
                var relationTypes = _types.Values.Where(e => e is RelationType);
                // todo: refactor
                result.AddRange(relationTypes);

                _ontology = new OntologyModel(result);
                _types.Clear();

                return _ontology;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void Invalidate()
        {
            _locker.EnterWriteLock();
            try
            {
                _ontology = null;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        private Dictionary<Guid, NodeType> _types = new Dictionary<Guid, NodeType>();
        private NodeType MapType(NodeTypeEntity ctxType)
        {
            var types = mapType(ctxType);
            return types;

            NodeType mapType(NodeTypeEntity type)
            {
                if (_types.ContainsKey(type.Id))
                    return _types[type.Id];

                if (type.Kind == Kind.Attribute)
                {
                    var attributeType = type.AttributeType;
                    var attr = new AttributeType(type.Id, type.Name, attributeType.ScalarType);
                    _types.Add(type.Id, attr);
                    FillProperties(type, attr);
                    attr.Meta = attr.CreateMeta(); // todo: refactor meta creation
                    return attr;
                }
                if (type.Kind == Kind.Entity)
                {
                    var entity = new EntityType(type.Id, type.Name, type.IsAbstract);
                    _types.Add(type.Id, entity);
                    FillProperties(type, entity);
                    // Process relation inheritance first
                    foreach (var outgoingRelation in type.OutgoingRelations.Where(r => r.Kind == RelationKind.Inheritance && !r.NodeType.IsArchived))
                        entity.AddType(mapRelation(outgoingRelation));
                    entity.Meta = entity.CreateMeta(); // todo: refactor. Creates meta with all parent types meta
                    foreach (var outgoingRelation in type.OutgoingRelations.Where(r => r.Kind != RelationKind.Inheritance && !r.NodeType.IsArchived))
                        entity.AddType(mapRelation(outgoingRelation));
                    return entity;
                }
                throw new Exception("Unsupported type.");
            }

            NodeType mapRelation(RelationTypeEntity relationType)
            {
                if (_types.ContainsKey(relationType.Id))
                    return _types[relationType.Id];

                var type = relationType.NodeType;
                var relation = default(RelationType);
                if (relationType.Kind == RelationKind.Embedding)
                {
                    relation = new EmbeddingRelationType(type.Id, type.Name, relationType.EmbeddingOptions);
                    FillProperties(type, relation);
                    _types.Add(type.Id, relation);
                    var target = mapType(relationType.TargetType);
                    relation.AddType(target);
                    relation.Meta = relation.CreateMeta(); // todo: refactor meta creation
                    _addInversedRelation(relation, _types[relationType.SourceTypeId]);
                }
                else if (relationType.Kind == RelationKind.Inheritance)
                {
                    relation = new InheritanceRelationType(type.Id);
                    FillProperties(type, relation);
                    _types.Add(type.Id, relation);
                    var target = mapType(relationType.TargetType);
                    relation.AddType(target);
                    relation.Meta = relation.CreateMeta(); // todo: refactor meta creation
                }
                else throw new ArgumentException("Unsupported relation kind");

                return relation;
            }
        }

        protected RelationType _addInversedRelation(RelationType relation, NodeType sourceType)
        {
            if (!(relation is EmbeddingRelationType relationType) || !relationType.HasInversed())
                return null;

            var meta = relationType.GetInversed();
            var embeddingOptions = meta.Multiple ? EmbeddingOptions.Multiple : EmbeddingOptions.Optional;
            var name = meta.Code ?? sourceType.Name.ToLowerCamelcase();
            var inversedRelation = new EmbeddingRelationType(Guid.NewGuid(), name, embeddingOptions, isInversed: true)
            {
                Title = meta.Title ?? sourceType.Title ?? name
            };

            inversedRelation.AddType(sourceType);
            inversedRelation.AddType(relationType);
            relationType.TargetType.AddType(inversedRelation);

            if (inversedRelation != null)
                _types.Add(inversedRelation.Id, inversedRelation);

            return inversedRelation;
        }

        private static void FillProperties(NodeTypeEntity type, NodeType ontologyType)
        {
            ontologyType.Title = type.Title;
            ontologyType.MetaSource = type.Meta == null ? null : JObject.Parse(type.Meta);
            ontologyType.CreatedAt = type.CreatedAt;
            ontologyType.UpdatedAt = type.UpdatedAt;
            ontologyType.UniqueValueFieldName = type.UniqueValueFieldName;
        }
    }
}
