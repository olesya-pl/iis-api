using System;
using System.Linq;
using System.Collections.Generic;

namespace Iis.Utility
{
    public static class LinqExtensions
    {
        public static IReadOnlyCollection<TItem> AsReadOnlyCollection<TItem>(this TItem item) => new[] { item };

        public static (IReadOnlyCollection<TItem> Added, IReadOnlyCollection<TItem> Removed) GetChanges<TItem>(this IEnumerable<TItem> first, IEnumerable<TItem> second) =>
            (first.Except(second).ToArray(), second.Except(first).ToArray());

        public static bool AnyDuplicate<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property) =>
            collection.GroupBy(property).Any(_ => _.Count() > 1);

        public static bool HaveSame<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property) =>
            collection.Select(property).Distinct().Count() == 1;

        public static IEnumerable<T> Except<T, TProperty>(this IEnumerable<T> first, IEnumerable<T> second,
            Func<T, TProperty> property) =>
            first.Except(second, new GenericEqualityComparer<T, TProperty>(property));

        public static IEnumerable<(TLeft Left, TRight Right)> FullOuterJoin<TLeft, TRight>
            (this IEnumerable<TLeft> leftCollection, IEnumerable<TRight> rightCollection,
            Func<TLeft, object> leftProperty, Func<TRight, object> rightProperty)
        {
            var leftJoin =
                from left in leftCollection
                join right in rightCollection on leftProperty(left) equals rightProperty(right) into se
                from p in se.DefaultIfEmpty()
                select (Left: left, Right: p);
            var rightJoin =
                from right in rightCollection
                join left in leftCollection on rightProperty(right) equals leftProperty(left) into se
                from p in se.DefaultIfEmpty()
                select (Left: p, Right: right);
            var union = leftJoin.Union(rightJoin);
            return union;
        }

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
