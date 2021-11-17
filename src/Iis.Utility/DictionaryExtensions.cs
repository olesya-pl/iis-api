using System.Linq;
using System.Collections.Generic;
namespace Iis.Utility
{
    public static class DictionaryExtensions
    {
        public static void TryAddRange<TKey, TValue>(this IDictionary<TKey, TValue> destination,  IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (destination is null) return;

            if (collection is null || !collection.Any()) return;

            foreach (var element in collection)
            {
                destination.TryAdd(element.Key, element.Value);
            }
        }
    }
}