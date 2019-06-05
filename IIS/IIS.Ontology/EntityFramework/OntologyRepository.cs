using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core;
using IIS.Core.Ontology;
using Microsoft.EntityFrameworkCore;

namespace IIS.Ontology.EntityFramework
{
    public class OntologyRepository : IOntology
    {
        private readonly ContourContext _context;
        private readonly IOSchema _schemaProvider;
        private TypeEntity _schema;

        public OntologyRepository(ContourContext context, IOSchema schema)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _schemaProvider = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string typeName, int limit = int.MaxValue, int offset = 0)
        {
            var data = await _context.Entities
                .Where(e => e.Type.Code == typeName && e.DeletedAt == null)
                .Include(e => e.Type)
                .Include(e => e.AttributeValues).ThenInclude(a => a.Attribute)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Target).ThenInclude(e => e.Type)
                .Include(e => e.ForwardRelations).ThenInclude(e => e.Target).ThenInclude(e => e.AttributeValues).ThenInclude(e => e.Attribute)
                //.Skip(offset).Take(limit)
                .ToArrayAsync();

            if (!data.Any()) return Enumerable.Empty<Entity>();

            data = data
                .Select(e =>
                {
                    e.AttributeValues = e.AttributeValues.Where(a => a.DeletedAt == null).ToArray();
                    e.ForwardRelations = e.ForwardRelations.Where(r => r.DeletedAt == null)
                    .Select(fr =>
                    {
                        fr.Target.AttributeValues = fr.Target.AttributeValues.Where(a => a.DeletedAt == null).ToArray();
                        return fr;
                    })
                    .ToArray();
                    return e;
                })
                .ToArray();
            var schema = _schema = await _schemaProvider.GetRootAsync();
            var types = schema.GetEntities();
            var entities = data.Select(e => MapEntity(types.Single(t => t.Name == typeName), e, 3))
                .Where(e => e != null)
                .ToArray();
            return entities;
        }

        //public async Task<IDictionary<(long, string), IOntologyNode>> GetEntitiesByAsync(IEnumerable<(long, string)> entityIds)
        //{
        //    var schema = _schema = await _schemaProvider.GetRootAsync();
        //    var data = await _context.Relations
        //        .Where(r => entityIds.Any(e => e.Item1 == r.SourceId && e.Item2 == r.Type.Code))
        //        .Include(r => r.Type)
        //        .Include(r => r.Target).ThenInclude(t => t.AttributeValues).ThenInclude(a => a.Attribute)
        //        .Include(r => r.Source)
        //        .ToArrayAsync()
        //        ;
        //    var types = schema.GetEntities();
        //    var entities = data
        //        .GroupBy(r => r.SourceId)
        //        .Select(r => r.First().Source)
        //        .Select(e => MapEntity(types.Single(t => t.Name == e.Type.Code), e));

        //    var nodes = new Dictionary<(long, string), IOntologyNode>();
        //    foreach (var entity in entities)
        //    {
        //        foreach (var relation in entity.RelationNames)
        //        {
        //            nodes.Add((entity.Id, relation), entity.GetRelation(relation));
        //        }
        //    }

        //    return nodes;
        //}

        //public async Task<IDictionary<string, ArrayRelation>> GetEntitiesAsync(IEnumerable<string> typeNames)
        //{
        //    var upperNames = typeNames.Select(_ => _.Camelize());
        //    var schema = _schema = await _schemaProvider.GetRootAsync();
        //    var types = schema.GetEntities();
        //    var data = await _context.Entities
        //        .Where(e => upperNames.Any(typeName => e.Type.Code == typeName || e.Type.Parent.Code == typeName))
        //        .Include(e => e.Type)
        //        .Include(e => e.AttributeValues).ThenInclude(a => a.Attribute)
        //        .ToArrayAsync();

        //    var list = new List<ArrayRelation>();
        //    foreach (var group in data.GroupBy(d => d.Type.Code))
        //    {
        //        var type = types.Single(t => t.Name == group.Key);
        //        var constraint = schema.GetConstraint(group.Key.ToLowerCamelcase());
        //        var entities = group.Select(e => MapEntity(type, e));
        //        var arrayRelation = new ArrayRelation(constraint, entities);
        //        list.Add(arrayRelation);
        //    }

        //    return list.ToDictionary(_ => _.Schema.Name);
        //}

        private Entity MapEntity(TypeEntity type, OEntity srcEntity, int depth = -1)
        {
            if (depth == 0) return null;
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
                            var targetEntity = MapEntity((TypeEntity)constraint.Target, relation.Target, depth - 1);
                            if (targetEntity is null) continue;
                            entity.AddRelation(new Relation(constraint, targetEntity));
                        }
                    }
                    else
                    {
                        var targetEntity = MapEntity((TypeEntity)constraint.Target, group.Single().Target, depth - 1);
                        if (targetEntity is null) continue;
                        entity.SetRelation(new Relation(constraint, targetEntity));
                    }
                }
                else if (type.HasUnion(constraintName))
                {
                    var group = relationsByName.SingleOrDefault(g => g.Key == constraintName);
                    if (group == null) continue;

                    var constraint = type.GetConstraint(constraintName);
                    var unionClass = (UnionClass)constraint.Target;
                    var union = new Union(unionClass);
                    // non array
                    var relation = group.Single();
                    var newSrcEntity = relation.Target;
                    var newSrcEntityType = unionClass.Classes.Single(u => u.Name == newSrcEntity.Type.Code);
                    var targetEntity = MapEntity(newSrcEntityType, newSrcEntity, depth - 1);
                    if (targetEntity is null) continue;
                    union.AddEntity(targetEntity);
                    entity.SetRelation(new Relation(constraint, union));
                    // todo: GetRelationInfo(e)
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

    }
}
