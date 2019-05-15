using System.Collections.Generic;
using System.Linq;

namespace IIS.OSchema
{
    public static class EntityExtensions
    {
        public static void AddAttribute(this Entity entity, string constraintName, IEnumerable<AttributeValue> values)
        {
            var constraint = entity.Type.GetAttribute(constraintName);
            if (constraint.IsArray) entity.AddRelation(new AttributeRelation(constraint, values));
            else entity.AddRelation(new AttributeRelation(constraint, values.Single()));
        }

        public static void AddEntity(this Entity entity, string constraintName, IEnumerable<EntityValue> entities)
        {
            var constraint = entity.Type.GetEntity(constraintName);
            if (constraint.IsArray) entity.AddRelation(new EntityRelation(constraint, entities));
            else entity.AddRelation(new EntityRelation(constraint, entities.Single()));
        }

        public static void AddUnion(this Entity entity, string constraintName, IEnumerable<EntityValue> entities)
        {
            var constraint = entity.Type.GetUnion(constraintName);
            entity.AddRelation(new UnionRelation(constraint, entities));
        }

        public static AttributeRelation GetAttributeRelation(this Entity entity, string constraintName) =>
            (AttributeRelation)entity.GetRelation(constraintName);

        public static EntityRelation GetEntityRelation(this Entity entity, string constraintName) =>
            (EntityRelation)entity.GetRelation(constraintName);

        public static UnionRelation GetUnionRelation(this Entity entity, string constraintName) =>
            (UnionRelation)entity.GetRelation(constraintName);
    }
}
