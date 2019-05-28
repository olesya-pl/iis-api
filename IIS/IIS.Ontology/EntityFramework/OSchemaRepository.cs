using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// todo: remove usage of GraphQL from this layer
using GraphQL.DataLoader;
using IIS.Core;
using IIS.Core.Resolving;
using Microsoft.EntityFrameworkCore;

namespace IIS.Ontology.EntityFramework
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

        public async Task<IDictionary<(long, string), IOntologyNode>> GetEntitiesByAsync(IEnumerable<(long, string)> entityIds)
        {
            var schema = await GetRootAsync();
            var data = await _context.Relations
                .Where(r => entityIds.Any(e => e.Item1 == r.SourceId && e.Item2 == r.Type.Code))
                .Include(r => r.Type)
                .Include(r => r.Target).ThenInclude(t => t.AttributeValues).ThenInclude(a => a.Attribute)
                .Include(r => r.Source)
                .ToArrayAsync()
                ;
            var types = schema.GetEntities();
            var entities = data
                .GroupBy(r => r.SourceId)
                .Select(r => r.First().Source)
                .Select(e => MapEntity(types.Single(t => t.Name == e.Type.Code), e));

            //var data = await _context.Entities.Where(_ => entityIds.Contains(_.Id))
                //.Include(e => e.Type)
                //.Include(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                //.Include(e => e.ForwardRelations).ThenInclude(e => e.Type)
                //.Include(e => e.ForwardRelations).ThenInclude(e => e.Target)
                //.ToArrayAsync();
            
            var nodes = new Dictionary<(long, string), IOntologyNode>();
            foreach (var entity in entities)
            {
                foreach (var relation in entity.RelationNames)
                {
                    nodes.Add((entity.Id, relation), entity.GetRelation(relation));
                }
            }
            //var relations = entities
            //.Select(e => new Relation(schema.GetConstraint(e.Type.Code.ToLowerCamelcase()), 
            //        MapEntity(types.Single(t => t.Name == e.Type.Code), e)
            //    )
            //).ToArray();

            return nodes;
        }

        public async Task<IDictionary<string, ArrayRelation>> GetEntitiesAsync(IEnumerable<string> typeNames)
        {
            var upperNames = typeNames.Select(_ => _.Camelize());
            var schema = await GetRootAsync();
            var types = schema.GetEntities();
            var data = await _context.Entities
                .Where(e => upperNames.Any(typeName => e.Type.Code == typeName || e.Type.Parent.Code == typeName))
                .Include(e => e.Type)
                .Include(e => e.AttributeValues).ThenInclude(a => a.Attribute)
                .ToArrayAsync();

            var list = new List<ArrayRelation>();
            foreach (var group in data.GroupBy(d => d.Type.Code))
            {
                var type = types.Single(t => t.Name == group.Key);
                var constraint = schema.GetConstraint(group.Key.ToLowerCamelcase());
                var relations = group.Select(e => MapEntity(type, e))
                    .Select(e => new Relation(constraint, e));
                var arrayRelation = new ArrayRelation(constraint, relations);
                list.Add(arrayRelation);
            }

            return list.ToDictionary(_ => _.Schema.Name);
        }

        private Entity MapEntity(TypeEntity type, OEntity srcEntity)
        {
            var concreteType = _schema.GetEntity(srcEntity.Type.Code.ToLowerCamelcase());
            var entity = new Entity(concreteType);
            entity.SetAttribute("id", srcEntity.Id);
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

                    var constraint = type.GetConstraint(constraintName);
                    if (constraint.IsArray)
                        foreach (var value in group) entity.AddAttribute(constraintName, value.Value, value.Id);
                    else entity.SetAttribute(constraintName, group.Single().Value);
                }
                else if (type.HasEntity(constraintName))
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var constraint = type.GetConstraint(constraintName);
                    if (constraint.IsArray)
                    {
                        // todo: GetRelationInfo(e)
                        foreach (var relation in group)
                        {
                            var targetEntity = MapEntity((TypeEntity)constraint.Target, relation.Target);
                            entity.AddRelation(new Relation(constraint, targetEntity));
                        }
                    }
                    else
                    {
                        var targetEntity = MapEntity((TypeEntity)constraint.Target, group.Single().Target);
                        entity.SetRelation(new Relation(constraint, targetEntity));
                    }
                }
                else if (type.HasUnion(constraintName))
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var constraint = type.GetConstraint(constraintName);
                    var union = new Union((UnionClass)constraint.Target);
                    // todo: GetRelationInfo(e)
                    foreach (var relation in group)
                    {
                        var targetEntity = MapEntity((TypeEntity)constraint.Target, relation.Target);
                        union.AddEntity(targetEntity);
                        entity.SetRelation(new Relation(constraint, union));
                    }
                }
            }
            return entity;
        }

        private Entity GetRelationInfo(ORelation e)
        {
            var type = _schema.GetEntity("_relationInfo");
            var relationInfo = new Entity(type);
            relationInfo.SetAttribute("id", e.TargetId);
            relationInfo.SetAttribute("startsAt", e.StartsAt);
            relationInfo.SetAttribute("endsAt", e.EndsAt);
            relationInfo.SetAttribute("createdAt", e.CreatedAt);
            relationInfo.SetAttribute("isInferred", e.IsInferred);

            return relationInfo;
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
            var attrResolver = new AttributeResolver();
            var relationInfo = new TypeEntity("RelationInfo");
            relationInfo.AddAttribute("id", ScalarType.Int, true, false, attrResolver);
            relationInfo.AddAttribute("startsAt", ScalarType.Date, false, false, attrResolver);
            relationInfo.AddAttribute("endsAt", ScalarType.Date, false, false, attrResolver);
            relationInfo.AddAttribute("createdAt", ScalarType.Date, true, false, attrResolver);
            relationInfo.AddAttribute("isInferred", ScalarType.Int, true, false, attrResolver);
            _types.Add(relationInfo.Name, relationInfo);
            schemaType.AddType("_relationInfo", relationInfo, true, true, new RelationInfoResolver());
            var resolver = new EntitiesResolver(this, _contextAccessor);
            foreach (var item in data)
            {
                var type = MapType(item);
                schemaType.AddType(type.Name.ToLowerCamelcase(), type, true, true, resolver);
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
            //type.IndexConfig = typeEntity.Meta.ContainsKey("index") ?
            //typeEntity.Meta.GetValue("index") as JObject : new JObject();
            //var relationResolver = new (this, _contextAccessor);
            var relationInfo = _types["RelationInfo"];
            type.AddType("_relationInfo", relationInfo, false, false, new RelationInfoResolver());
            type.AddType("id", new AttributeClass("id", ScalarType.Int), true, false, new AttributeResolver());

            var constraintsByName = typeEntity.ForwardRestrictions.GroupBy(r => r.Type.Code);
            foreach (var constraints in constraintsByName)
            {
                var name = constraints.Key;
                var constraint = constraints.First();
                var isArray = constraint.IsMultiple;
                var isRequired = constraint.IsRequired;
                var kind = constraints.Count() > 1 ? Kind.Union : Kind.Class;

                if (kind == Kind.Class)
                {
                    var resolver = new EntityRelationResolver(this, _contextAccessor);
                    var target = MapType(constraint.Target);
                    type.AddType(name, target, isRequired, isArray, resolver);
                }
                else if (kind == Kind.Union)
                {
                    var resolver = new EntityRelationResolver(this, _contextAccessor);
                    var targets = constraints.Select(e => MapType(e.Target));
                    var target = new UnionClass($"{type.Name}{name.Camelize()}RelationUnion", targets);
                    type.AddType(name, target, isRequired, isArray, resolver);
                }
            }

            foreach (var constraint in typeEntity.AttributeRestrictions)
            {
                var resolver = new AttributeResolver();
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
