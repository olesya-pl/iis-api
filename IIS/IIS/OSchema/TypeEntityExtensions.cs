using System.Collections.Generic;
using System.Linq;

namespace IIS.OSchema
{
    public static class TypeEntityExtensions
    {
        private static TConstraint AddGenericConstraint<TConstraint>(this TypeEntity type, TConstraint constraint)
            where TConstraint: Constraint => (TConstraint)type.AddConstraint(constraint);

        public static AttributeConstraint AddAttribute(this TypeEntity type, string name, ScalarType attrType, 
            bool isRequired, bool isArray = false, IRelationResolver resolver = null) => 
            type.AddGenericConstraint(new AttributeConstraint(name, attrType, isRequired, isArray, resolver));

        public static EntityConstraint AddEntity(this TypeEntity type, string name, TypeEntity target, bool isRequired, 
            bool isArray, IRelationResolver resolver = null) => 
            type.AddGenericConstraint(new EntityConstraint(name, target, isRequired, isArray, resolver));

        public static UnionConstraint AddUnion(this TypeEntity type, string name, IEnumerable<TypeEntity> types, 
            bool isRequired, bool isArray = false, IRelationResolver resolver = null) => 
            type.AddGenericConstraint(new UnionConstraint(name, types, isRequired, isArray, resolver));

        public static bool HasAttribute(this TypeEntity type, string name) => 
            type.GetConstraint(name).Kind == TargetKind.Attribute;

        public static bool HasEntity(this TypeEntity type, string name) => 
            type.GetConstraint(name).Kind == TargetKind.Entity;

        public static bool HasUnion(this TypeEntity type, string name) => 
            type.GetConstraint(name).Kind == TargetKind.Union;

        public static AttributeConstraint GetAttribute(this TypeEntity type, string name) => 
            (AttributeConstraint)type.GetConstraint(name);

        public static EntityConstraint GetEntity(this TypeEntity type, string name) => 
            (EntityConstraint)type.GetConstraint(name);

        public static UnionConstraint GetUnion(this TypeEntity type, string name) => 
            (UnionConstraint)type.GetConstraint(name);

        public static IEnumerable<EntityConstraint> GetEntities(this TypeEntity type) => 
            type.Constraints.OfType<EntityConstraint>();
    }
}
