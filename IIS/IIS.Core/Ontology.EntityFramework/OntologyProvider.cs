using System;
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
    public class OntologyProvider : IOntologyProvider
    {
        private readonly OntologyContext _context;
        private Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();

        public OntologyProvider(OntologyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            var types = await _context.Types.Where(e => !e.IsArchived && e.Kind != Kind.Relation)
                .Include(e => e.IncomingRelations).ThenInclude(e => e.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Type)
                .Include(e => e.AttributeType)
                .ToArrayAsync(cancellationToken);

            var result = types.Select(MapType).ToList();

            // todo: refactor
            result.AddRange(_types.Values.Where(e => e is RelationType));

            return result;
        }

        private Type MapType(Context.Type type)
        {
            if (_types.ContainsKey(type.Id))
                return _types[type.Id];

            if (type.Kind == Kind.Attribute)
            {
                var attributeType = type.AttributeType;
                var attr = new AttributeType(type.Id, type.Name, MapScalarType(attributeType.ScalarType));
                _types.Add(type.Id, attr);
                FillProperties(type, attr);
                return attr;
            }
            if (type.Kind == Kind.Entity)
            {
                var entity = new EntityType(type.Id, type.Name, type.IsAbstract);
                _types.Add(type.Id, entity);
                FillProperties(type, entity);
                foreach (var outgoingRelation in type.OutgoingRelations)
                {
                    var relation = MapRelation(outgoingRelation);
                    entity.AddType(relation);
                }
                return entity;
            }
            throw new Exception("Unsupported type.");
        }

        private Type MapRelation(Context.RelationType relationType)
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
                var target = MapType(relationType.TargetType);
                relation.AddType(target);
            }
            if (relationType.Kind == RelationKind.Inheritance)
            {
                relation = new InheritanceRelationType(type.Id);
                FillProperties(type, relation);
                _types.Add(type.Id, relation);
                var target = MapType(relationType.TargetType);
                relation.AddType(target);
            }

            return relation;
        }

        private static void FillProperties(Context.Type type, Type ontologyType)
        {
            ontologyType.Title = type.Title;
            ontologyType.Meta = type.Meta == null ? null : JObject.Parse(type.Meta);
            //ontologyType.TypeMeta = ontologyType.CreateMeta(); // todo: remake meta creation
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
