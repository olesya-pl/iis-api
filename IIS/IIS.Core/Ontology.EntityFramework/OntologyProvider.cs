using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology.EntityFramework
{
    public class OntologyProvider : IOntologyProvider
    {
        private readonly OntologyContext _context;

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

            var result = types.Select(MapType).ToArray();

            return result;
        }

        private static Type MapType(Context.Type type)
        {
            if (type.Kind == Kind.Attribute)
            {
                var attributeType = type.AttributeType;
                var attr = new AttributeType(type.Id, type.Name, attributeType.ScalarType.ToString()) { Title = type.Title};
                return attr;
            }
            if (type.Kind == Kind.Entity)
            {
                var entity = new EntityType(type.Id, type.Name, type.IsAbstract) {
                    Title = type.Title,
                    Meta = type.Meta == null ? null : JObject.Parse(type.Meta),
                    CreatedAt = type.CreatedAt, UpdatedAt = type.UpdatedAt};
                foreach (var outgoingRelation in type.OutgoingRelations)
                {
                    var relation = MapRelation(outgoingRelation);
                    entity.AddType(relation);
                }
                return entity;
            }
            throw new Exception("Unsupported type.");
        }

        private static RelationType MapRelation(Context.RelationType relationType)
        {
            var type = relationType.Type;
            var isRequired = false; // todo: map from type.Meta
            var relation = default(RelationType);
            if (relationType.Kind == RelationKind.Embedding)
            {
                relation = new EmbeddingRelationType(type.Id, type.Name, isRequired, relationType.IsArray) { Title = type.Title};
                var target = MapType(relationType.TargetType);
                relation.AddType(target);
            }
            if (relationType.Kind == RelationKind.Inheritance)
            {
                relation = new InheritanceRelationType(type.Id);
                var target = MapType(relationType.TargetType);
                relation.AddType(target);
            }
            return relation;
        }
    }
}
