using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IIS.OSchema.EntityFramework
{
    public class OSchemaRepository : IOSchema
    {
        private readonly ContourContext _context;

        public OSchemaRepository(ContourContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TypeEntity> GetRootAsync()
        {
            var data = await _context.Types
                .Include(_ => _.Parent)
                .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
                .OfType<OTypeEntity>()
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Type)
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Target)
                .ToArrayAsync();

            var types = data.Select(MapType).ToArray();
            var constraints = types.Select(t => new EntityConstraint(t.Name, t, true, true));
            var schemaType = new TypeEntity("Entities", constraints);

            return schemaType;
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string typeName)
        {
            var schema = await GetRootAsync();
            var types = schema.Constraints.Select(c => ((EntityConstraint)c).Target);
            var data = await _context.Entities
                .Where(e => e.Type.Code == typeName || e.Type.Parent.Code == typeName)
                .Include(e => e.Type)
                .Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Type)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Target)
                .ToArrayAsync();

            var entities = data
                .Select(e => MapEntity(types.Single(t => t.Name == e.Type.Code), e))
                .ToArray();
            
            return entities;
        }

        private static Entity MapEntity(TypeEntity type, OEntity e)
        {
            return new Entity(type, e.Id, MapRelations(type, e));
        }

        private static IEnumerable<Relation> MapRelations(TypeEntity type, OEntity entity)
        {
            var list = new List<Relation>();
            var attributesByName = entity.AttributeValues.GroupBy(a => a.Attribute.Code);
            var relationsByName = entity.ForwardRelations.GroupBy(r => r.Type.Code);
            foreach (var constraint in type.Constraints)
            {
                if (constraint.Kind == TargetKind.Attribute)
                {
                    var group = attributesByName.SingleOrDefault(g => g.Key == constraint.Name);
                    if (group == null) continue;

                    var attributeConstraint = type.GetAttribute(constraint.Name);
                    var values = group.Select(g => new AttributeValue(g.Id, g.Value));
                    if (constraint.IsArray)
                    {
                        list.Add(new AttributeRelation(attributeConstraint, values));
                    }
                    else
                    {
                        list.Add(new AttributeRelation(attributeConstraint, values.Single()));
                    }
                }
                else if (constraint.Kind == TargetKind.Entity)
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraint.Name);
                    if (group == null) continue;
                    var entityConstraint = type.GetEntity(constraint.Name);
                    var entities = group.Select(e => MapEntity(entityConstraint.Target, e.Target));
                    if (constraint.IsArray)
                    {
                        list.Add(new EntityRelation(entityConstraint, entities));
                    }
                    else
                    {
                        list.Add(new EntityRelation(entityConstraint, entities.Single()));
                    }
                }
                else if (constraint.Kind == TargetKind.Union)
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraint.Name);
                    if (group == null) continue;
                    var unionConstraint = type.GetUnion(constraint.Name);
                    var entities = group.Select(e => 
                        MapEntity(unionConstraint.Targets.Single(t => t.Name == e.Target.Type.Code), e.Target));
                    list.Add(new UnionRelation(unionConstraint, entities));
                }
            }

            return list;
        }

        private static readonly Dictionary<string, TypeEntity> _types = new Dictionary<string, TypeEntity>();
        private static TypeEntity MapType(OTypeEntity typeEntity)
        {
            if (_types.ContainsKey(typeEntity.Code)) return _types[typeEntity.Code];

            var parent = typeEntity.Parent == null ? null : MapType(typeEntity.Parent);
            var type = _types[typeEntity.Code] = new TypeEntity(typeEntity.Code, typeEntity.IsAbstract, parent);

            type.AddConstraint(new AttributeConstraint("id", ScalarType.Int, true));
            var relationInfo = new TypeEntity("RelationInfo");
            relationInfo.AddConstraint(new AttributeConstraint("id", ScalarType.Int, true));
            relationInfo.AddConstraint(new AttributeConstraint("startsAt", ScalarType.Date, false));
            relationInfo.AddConstraint(new AttributeConstraint("endsAt", ScalarType.Date, false));
            relationInfo.AddConstraint(new AttributeConstraint("createdAt", ScalarType.Date, true));
            relationInfo.AddConstraint(new AttributeConstraint("isInferred", ScalarType.Int, true));
            type.AddConstraint(new EntityConstraint("_relation", relationInfo, true, false));

            var constraintsByName = typeEntity.ForwardRestrictions.GroupBy(r => r.Type.Code);
            foreach (var constraints in constraintsByName)
            {
                var name = constraints.Key;
                var constraint = constraints.First();
                var isArray = constraint.IsMultiple;
                var isRequired = constraint.IsRequired;
                var kind = constraints.Count() > 1 ? TargetKind.Union : TargetKind.Entity;

                if (kind == TargetKind.Entity)
                {
                    var target = MapType(constraint.Target);
                    type.AddConstraint(new EntityConstraint(name, target, isRequired, isArray));
                }
                else if (kind == TargetKind.Union)
                {
                    var targets = constraints.Select(e => MapType(e.Target));
                    type.AddConstraint(new UnionConstraint(name, targets, isRequired, isArray));
                }
            }

            foreach (var constraint in typeEntity.AttributeRestrictions)
            {
                type.AddConstraint(new AttributeConstraint(
                        constraint.Attribute.Code,
                        constraint.Attribute.Type,
                        constraint.IsRequired,
                        constraint.IsMultiple
                    )
                );
            }

            return type;
        }
    }
}
