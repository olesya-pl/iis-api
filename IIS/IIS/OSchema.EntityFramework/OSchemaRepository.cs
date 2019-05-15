using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using IIS.OSchema.Resolving;
using Microsoft.EntityFrameworkCore;

namespace IIS.OSchema.EntityFramework
{
    public class OSchemaRepository : IOSchema
    {
        private readonly IDataLoaderContextAccessor _contextAccessor;
        private readonly ContourContext _context;
        private TypeEntity _schema;

        public OSchemaRepository(ContourContext context, IDataLoaderContextAccessor contextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public async Task<IDictionary<long, EntityValue>> GetEntitiesByAsync(IEnumerable<long> entityIds)
        {
            var schema = await GetRootAsync();
            var types = schema.GetEntities().Select(_ => _.Target);
            var data = await _context.Entities.Where(_ => entityIds.Contains(_.Id))
                .Include(e => e.Type)
                .Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Type)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Target)
                .ToArrayAsync();
            var entities = data
                .Select(e => new EntityValue(MapEntity(types.Single(t => t.Name == e.Type.Code), e), Enumerable.Empty<AttributeRelation>()))
                .ToArray();
            return entities.ToDictionary(_ => _.Value.Id);
        }

        public async Task<IDictionary<string, IEnumerable<EntityValue>>> GetEntitiesAsync(IEnumerable<string> typeNames)
        {
            var schema = await GetRootAsync();
            var types = schema.GetEntities().Select(e => e.Target);
            var data = await _context.Entities
                .Where(e => typeNames.Any(typeName => e.Type.Code == typeName || e.Type.Parent.Code == typeName))
                .Include(e => e.Type)
                .ToArrayAsync();
            
            var entities = data
                .Select(e => new EntityValue(MapEntity(types.Single(t => t.Name == e.Type.Code), e), Enumerable.Empty<AttributeRelation>()))
                .ToArray();

            return entities.GroupBy(_ => _.Value.Type.Name)
                .ToDictionary(_ => _.Key, _ => _.AsEnumerable());
        }

        private Entity MapEntity(TypeEntity type, OEntity srcEntity)
        {
            var concreteType = _schema.GetEntity(srcEntity.Type.Code).Target;
            var entity = new Entity(concreteType, srcEntity.Id);
            var attributesByName = srcEntity.AttributeValues
                .Where(e => e.DeletedAt == null)
                .GroupBy(a => a.Attribute.Code);
            var relationsByName = srcEntity.ForwardRelations.GroupBy(r => r.Type.Code);
            foreach (var constraintName in type.ConstraintNames)
            {
                if (type.HasAttribute(constraintName))
                {
                    var group = attributesByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var values = group.Select(g => new AttributeValue(g.Id, g.Value));
                    entity.AddAttribute(constraintName, values);
                }
                else if (type.HasEntity(constraintName))
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var constraint = type.GetEntity(constraintName);
                    var entities = group.Select(e => new EntityValue(MapEntity(constraint.Target, e.Target), GetRelationInfo(e)));
                    entity.AddEntity(constraintName, entities);
                }
                else if (type.HasUnion(constraintName))
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var constraint = type.GetUnion(constraintName);
                    var entities = group
                        .Select(e => 
                        new EntityValue(
                            MapEntity(constraint.Targets.Single(t => t.Name == e.Target.Type.Code), e.Target), GetRelationInfo(e)));
                    entity.AddUnion(constraintName, entities);
                }
            }
            return entity;
        }

        private IEnumerable<AttributeRelation> GetRelationInfo(ORelation e)
        {
            var type = _schema.GetEntity("_relationInfo").Target;
            var relationInfo = new Entity(type, e.Id);
            relationInfo.AddAttribute("startsAt", new[] { new AttributeValue(0, e.StartsAt) });
            relationInfo.AddAttribute("endsAt", new[] { new AttributeValue(0, e.EndsAt) });
            relationInfo.AddAttribute("createdAt", new[] { new AttributeValue(0, e.CreatedAt) });
            relationInfo.AddAttribute("isInferred", new[] { new AttributeValue(0, e.IsInferred) });

            return relationInfo.Relations.OfType<AttributeRelation>();
        }

        // Schema
        public async Task<TypeEntity> GetRootAsync()
        {
            if (_schema != null) return _schema;

            var data = await _context.Types
                .Include(_ => _.Parent)
                .Include(_ => _.AttributeRestrictions).ThenInclude(_ => _.Attribute)
                .OfType<OTypeEntity>()
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Type)
                .Include(_ => _.ForwardRestrictions).ThenInclude(_ => _.Target)
                .ToArrayAsync();
                
            var schemaType = new TypeEntity("Entities");
            var attrResolver = new RelationInfoAttributeResolver();
            var relationInfo = new TypeEntity("RelationInfo");
            relationInfo.AddAttribute("id", ScalarType.Int, true, false, attrResolver);
            relationInfo.AddAttribute("startsAt", ScalarType.Date, false, false, attrResolver);
            relationInfo.AddAttribute("endsAt", ScalarType.Date, false, false, attrResolver);
            relationInfo.AddAttribute("createdAt", ScalarType.Date, true, false, attrResolver);
            relationInfo.AddAttribute("isInferred", ScalarType.Int, true, false, attrResolver);
            _types.Add(relationInfo.Name, relationInfo);
            schemaType.AddEntity("_relationInfo", relationInfo, true, true, new RelationInfoResolver());
            var resolver = new EntitiesResolver(this, _contextAccessor);
            foreach (var item in data)
            {
                var type = MapType(item);
                schemaType.AddEntity(type.Name, type, true, true, resolver);
            }
            _schema = schemaType;
            return schemaType;
        }

        private static readonly Dictionary<string, TypeEntity> _types = new Dictionary<string, TypeEntity>();
        private TypeEntity MapType(OTypeEntity typeEntity)
        {
            if (_types.ContainsKey(typeEntity.Code)) return _types[typeEntity.Code];

            var parent = typeEntity.Parent == null ? null : MapType(typeEntity.Parent);
            var type = _types[typeEntity.Code] = new TypeEntity(typeEntity.Code, typeEntity.IsAbstract, parent);

            var relationInfo = _types["RelationInfo"];
            type.AddEntity("_relationInfo", relationInfo, false, false, new RelationInfoResolver());
            type.AddAttribute("id", ScalarType.Int, true, false, new EntityRelationResolver(this, _contextAccessor));

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
                    var resolver = new EntityRelationResolver(this, _contextAccessor);
                    var target = MapType(constraint.Target);
                    type.AddEntity(name, target, isRequired, isArray, resolver);
                }
                else if (kind == TargetKind.Union)
                {
                    var resolver = new EntityRelationResolver(this, _contextAccessor);
                    var targets = constraints.Select(e => MapType(e.Target));
                    type.AddUnion(name, targets, isRequired, isArray, resolver);
                }
            }

            foreach (var constraint in typeEntity.AttributeRestrictions)
            {
                var resolver = new EntityRelationResolver(this, _contextAccessor);
                type.AddAttribute(
                    constraint.Attribute.Code, 
                    constraint.Attribute.Type, 
                    constraint.IsRequired, 
                    constraint.IsMultiple, 
                    resolver);
            }

            return type;
        }
    }
}
