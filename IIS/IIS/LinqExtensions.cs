using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS
{
    public static class LinqExtensions
    {
        public static bool AnyDuplicate<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property) => 
            collection.GroupBy(property).Any(_ => _.Count() > 1);

        public static bool HaveSame<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property) => 
            collection.Select(property).Distinct().Count() == 1;
            
        public static IEnumerable<T> Except<T, TProperty>(this IEnumerable<T> first, IEnumerable<T> second,
            Func<T, TProperty> property) => 
            first.Except(second, new GenericEqualityComparer<T, TProperty>(property));

        private class GenericEqualityComparer<T, TProperty> : EqualityComparer<T>
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
}
