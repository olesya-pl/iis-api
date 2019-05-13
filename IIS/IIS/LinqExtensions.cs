using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS
{
    public static class LinqExtensions
    {
        public static bool HaveSame<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property)
            => collection.Any(outer => collection.Any(inner => property(inner).Equals(property(outer)))) || !collection.Any();

        public static bool HaveAllSame<T, TProperty>(this IEnumerable<T> collection, Func<T, TProperty> property)
            => collection.Any(outer => collection.Any(inner => !property(inner).Equals(property(outer)))) || !collection.Any();
    }
}
