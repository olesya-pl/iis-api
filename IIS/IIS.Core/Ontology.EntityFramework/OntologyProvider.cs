﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using IIS.Core.Ontology.Meta;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.EntityFramework
{
    public class OntologyProvider : BaseOntologyProvider, IOntologyProvider
    {
        private readonly ContextFactory _contextFactory;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private Ontology _ontology;

        public OntologyProvider(ContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default)
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
                        var types = context.Types.Where(e => !e.IsArchived && e.Kind != Kind.Relation)
                            .Include(e => e.IncomingRelations).ThenInclude(e => e.Type)
                            .Include(e => e.OutgoingRelations).ThenInclude(e => e.Type)
                            .Include(e => e.AttributeType)
                            .ToArray();
                        var result = types.Select(e => MapType(e)).ToList();
                        var relationTypes = _types.Values.Where(e => e is RelationType);
                        // todo: refactor
                        result.AddRange(relationTypes);

                        _ontology = new Ontology(result);
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

        private Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();
        private Type MapType(Context.Type ctxType)
        {
            var types = mapType(ctxType);
            return types;

            Type mapType(Context.Type type)
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

            Type mapRelation(Context.RelationType relationType)
            {
                if (_types.ContainsKey(relationType.Id))
                    return _types[relationType.Id];

                var type = relationType.Type;
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

        protected override RelationType _addInversedRelation(RelationType relation, Type sourceType)
        {
            var inversedRelation = base._addInversedRelation(relation, sourceType);

            if (inversedRelation != null)
                _types.Add(inversedRelation.Id, inversedRelation);

            return inversedRelation;
        }

        private static void FillProperties(Context.Type type, Type ontologyType)
        {
            ontologyType.Title = type.Title;
            ontologyType.MetaSource = type.Meta == null ? null : JObject.Parse(type.Meta);
            ontologyType.CreatedAt = type.CreatedAt;
            ontologyType.UpdatedAt = type.UpdatedAt;
        }

        private static ScalarType MapScalarType(Context.ScalarType scalarType)
        {
            switch (scalarType)
            {
                case Context.ScalarType.Boolean: return ScalarType.Boolean;
                case Context.ScalarType.Date: return ScalarType.DateTime;
                case Context.ScalarType.Decimal: return ScalarType.Decimal;
                case Context.ScalarType.File: return ScalarType.File;
                case Context.ScalarType.Geo: return ScalarType.Geo;
                case Context.ScalarType.Int: return ScalarType.Integer;
                case Context.ScalarType.String: return ScalarType.String;
                default: throw new NotImplementedException();
            }
        }

        private static EmbeddingOptions Map(Context.EmbeddingOptions embeddingOptions)
        {
            switch (embeddingOptions)
            {
                case Context.EmbeddingOptions.Optional: return EmbeddingOptions.Optional;
                case Context.EmbeddingOptions.Required: return EmbeddingOptions.Required;
                case Context.EmbeddingOptions.Multiple: return EmbeddingOptions.Multiple;
                default: throw new ArgumentOutOfRangeException(nameof(embeddingOptions), embeddingOptions, null);
            }
        }
    }
}
