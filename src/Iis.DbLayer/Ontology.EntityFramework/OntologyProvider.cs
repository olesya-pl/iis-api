﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;
using IIS.Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using EmbeddingOptions = Iis.Domain.EmbeddingOptions;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class OntologyProvider : BaseOntologyProvider, IOntologyProvider
    {
        private readonly ContextFactory _contextFactory;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private OntologyModel _ontology;

        public OntologyProvider(ContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<OntologyModel> GetOntologyAsync(CancellationToken cancellationToken = default)
        {
            // ReaderWriterLockSlim is used to allow multiple concurrent reads and only one exclusive write.
            // Class member can be replaced with some distributed cache
            return await Task.Run(() =>
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
                    using (var context = _contextFactory.CreateContext())
                    {
                        var types = context.NodeTypes.Where(e => !e.IsArchived && e.Kind != Kind.Relation)
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
                    }

                    return _ontology;
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }, cancellationToken);
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
                    var attr = new AttributeType(type.Id, type.Name, MapScalarType(attributeType.ScalarType));
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
                    foreach (var outgoingRelation in type.OutgoingRelations.Where(r => r.Kind == RelationKind.Inheritance))
                        entity.AddType(mapRelation(outgoingRelation));
                    entity.Meta = entity.CreateMeta(); // todo: refactor. Creates meta with all parent types meta
                    foreach (var outgoingRelation in type.OutgoingRelations.Where(r => r.Kind != RelationKind.Inheritance))
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
                    relation = new EmbeddingRelationType(type.Id, type.Name, Map(relationType.EmbeddingOptions));
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

        protected override RelationType _addInversedRelation(RelationType relation, NodeType sourceType)
        {
            var inversedRelation = base._addInversedRelation(relation, sourceType);

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
        }

        private static Domain.ScalarType MapScalarType(Interfaces.Ontology.Schema.ScalarType scalarType)
        {
            switch (scalarType)
            {
                case Interfaces.Ontology.Schema.ScalarType.Boolean: return Domain.ScalarType.Boolean;
                case Interfaces.Ontology.Schema.ScalarType.Date: return Domain.ScalarType.DateTime;
                case Interfaces.Ontology.Schema.ScalarType.Decimal: return Domain.ScalarType.Decimal;
                case Interfaces.Ontology.Schema.ScalarType.File: return Domain.ScalarType.File;
                case Interfaces.Ontology.Schema.ScalarType.Geo: return Domain.ScalarType.Geo;
                case Interfaces.Ontology.Schema.ScalarType.Int: return Domain.ScalarType.Integer;
                case Interfaces.Ontology.Schema.ScalarType.String: return Domain.ScalarType.String;
                default: throw new NotImplementedException();
            }
        }

        private static EmbeddingOptions Map(Interfaces.Ontology.Schema.EmbeddingOptions embeddingOptions)
        {
            switch (embeddingOptions)
            {
                case Interfaces.Ontology.Schema.EmbeddingOptions.Optional: return EmbeddingOptions.Optional;
                case Interfaces.Ontology.Schema.EmbeddingOptions.Required: return EmbeddingOptions.Required;
                case Interfaces.Ontology.Schema.EmbeddingOptions.Multiple: return EmbeddingOptions.Multiple;
                default: throw new ArgumentOutOfRangeException(nameof(embeddingOptions), embeddingOptions, null);
            }
        }
    }
}