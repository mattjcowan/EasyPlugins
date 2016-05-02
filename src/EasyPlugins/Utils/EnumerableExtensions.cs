using System;
using System.Collections.Generic;

namespace EasyPlugins.Utils
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {

            var index = 0;
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            IDictionary<TKey, TValue> range, bool replaceIfItemExists = false)
        {
            foreach (var item in range)
            {
                if (!dictionary.ContainsKey(item.Key))
                {
                    dictionary.Add(item.Key, item.Value);
                }
                else if (replaceIfItemExists)
                {
                    dictionary[item.Key] = item.Value;
                }
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue defaultValue)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }
    }
}
