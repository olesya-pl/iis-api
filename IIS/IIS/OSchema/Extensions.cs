using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.OSchema
{
    public static class Extensions
    {
        public static TConstraint GetConstraint<TConstraint>(this TypeEntity type, string name)
            where TConstraint : Constraint => (TConstraint)type[name];

        public static AttributeConstraint GetAttribute(this TypeEntity type, string name)
            => type.GetConstraint<AttributeConstraint>(name);

        public static EntityConstraint GetEntity(this TypeEntity type, string name)
            => type.GetConstraint<EntityConstraint>(name);

        public static UnionConstraint GetUnion(this TypeEntity type, string name)
            => type.GetConstraint<UnionConstraint>(name);


        public static IEnumerable<T> Except<T, TProperty>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TProperty> property)
            => first.Except(second, new GenericEqualityComparer<T, TProperty>(property));
    }

    public class GenericEqualityComparer<T, TProperty> : EqualityComparer<T>
    {
        private readonly Func<T, TProperty> _property;

        public GenericEqualityComparer(Func<T, TProperty> property)
        {
            _property = property;
        }

        public override bool Equals(T x, T y) => _property(x).Equals(_property(y));

        public override int GetHashCode(T obj) => _property(obj).GetHashCode();
    }
}
