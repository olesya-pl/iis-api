using System.Collections.Generic;
namespace Iis.Utility
{
    public static class DictionaryExtensions
    {
        public static void TryAddRange<TKey, TValue>(this IDictionary<TKey, TValue> destination, IDictionary<TKey, TValue> collection)
        {
            if (destination is null) return;

            if (collection is null || collection.Count == 0) return;

            foreach (var element in collection)
            {
                destination.TryAdd(element.Key, element.Value);
            }
        }
    }
}