using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public static class EntityExtensions
    {
        public static void SetAttribute(this Entity entity, string name, object value)
        {
            var type = entity.Schema;
            entity.SetRelation(new Relation(type.GetConstraint(name), new Attribute(type.GetAttribute(name), value)));
        }

        public static void AddAttribute(this Entity entity, string name, object value, Guid id)
        {
            var type = entity.Schema;
            var relation = new Relation(type.GetConstraint(name), new Attribute(type.GetAttribute(name), value, id));
            entity.AddRelation(relation);
        }

        //public static void AddEntity(this Object entity, string constraintName, IEnumerable<Object> entities)
        //{
        //    var constraint = entity.Schema.GetEntity(constraintName);
        //    if (constraint.IsArray) entity.AddRelation(new AggregatedRelation(constraint, entities));
        //    else entity.AddRelation(new Relation(constraint, entities.Single()));
        //}

        //public static void AddUnion(this Object entity, string constraintName, Union union)
        //{
        //    var constraint = entity.Schema.GetUnion(constraintName);
        //    entity.AddRelation(new Relation(constraint, union));
        //}

        //public static Attribute GetAttribute(this Object entity, string constraintName) =>
        //    (Attribute)entity.GetRelation(constraintName)?.Target;

        //public static Object GetEntity(this Object entity, string constraintName) =>
        //    (Object)entity.GetRelation(constraintName).Target;

        //public static Union GetUnion(this Object entity, string constraintName) =>
            //(Union)entity.GetRelation(constraintName).Target;
    }
}
