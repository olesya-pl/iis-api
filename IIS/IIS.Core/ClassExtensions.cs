using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public static class ClassExtensions
    {
        public static bool HasAttribute(this TypeEntity type, string constraintName) => 
            type.HasConstraint(constraintName) && type.GetConstraint(constraintName).Target.Kind == Kind.Attribute;

        public static bool HasEntity(this TypeEntity type, string constraintName) =>
            type.HasConstraint(constraintName) && type.GetConstraint(constraintName).Target.Kind == Kind.Class;

        public static bool HasUnion(this TypeEntity type, string constraintName) =>
            type.HasConstraint(constraintName) && type.GetConstraint(constraintName).Target.Kind == Kind.Union;

        public static AttributeClass GetAttribute(this TypeEntity type, string constraintName) => 
            (AttributeClass)type.GetConstraint(constraintName).Target;

        public static TypeEntity GetEntity(this TypeEntity type, string constraintName) => 
            (TypeEntity)type.GetConstraint(constraintName).Target;

        public static UnionClass GetUnion(this TypeEntity type, string constraintName) => 
            (UnionClass)type.GetConstraint(constraintName).Target;

        public static IEnumerable<AttributeClass> GetAttributes(this TypeEntity type) =>
            type.Constraints.Where(c => c.Target.Kind == Kind.Attribute).Select(c => (AttributeClass)c.Target);

        public static IEnumerable<TypeEntity> GetEntities(this TypeEntity type) => 
            type.Constraints.Where(c => c.Target.Kind == Kind.Class).Select(c => (TypeEntity)c.Target);

        public static IEnumerable<UnionClass> GetUnions(this TypeEntity type) =>
            type.Constraints.Where(c => c.Target.Kind == Kind.Union).Select(c => (UnionClass)c.Target);

        public static void AddAttribute(this TypeEntity type, string name, ScalarType scalarType, bool isRequired,
            bool isArray = false, IRelationResolver resolver = null) =>
            type.AddType(name, new AttributeClass(name, scalarType), isRequired, isArray, resolver);
    }
}
