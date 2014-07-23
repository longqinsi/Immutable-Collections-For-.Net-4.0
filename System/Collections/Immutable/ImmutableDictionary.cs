namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Validation;

    public static class ImmutableDictionary
    {
        public static bool Contains<TKey, TValue>(this IImmutableDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            Requires.NotNull<IImmutableDictionary<TKey, TValue>>(map, "map");
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return map.Contains(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>()
        {
            return ImmutableDictionary<TKey, TValue>.Empty;
        }

        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
        }

        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
        }

        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
        {
            return Create<TKey, TValue>().ToBuilder();
        }

        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
        {
            return Create<TKey, TValue>(keyComparer).ToBuilder();
        }

        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            return Create<TKey, TValue>(keyComparer, valueComparer).ToBuilder();
        }

        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.AddRange(items);
        }

        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
        }

        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetValueOrDefault<TKey, TValue>(key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue local;
            Requires.NotNull<IImmutableDictionary<TKey, TValue>>(dictionary, "dictionary");
            Requires.NotNullAllowStructs<TKey>(key, "key");
            if (dictionary.TryGetValue(key, out local))
            {
                return local;
            }
            return defaultValue;
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToImmutableDictionary<TKey, TValue>(null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
        {
            return source.ToImmutableDictionary<TKey, TValue>(keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ToImmutableDictionary<TSource, TKey, TSource>(keySelector, delegate (TSource v) {
                return v;
            }, null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(source, "source");
            ImmutableDictionary<TKey, TValue> dictionary = source as ImmutableDictionary<TKey, TValue>;
            if (dictionary != null)
            {
                return dictionary.WithComparers(keyComparer, valueComparer);
            }
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
        }

        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            return source.ToImmutableDictionary<TSource, TKey, TSource>(keySelector, delegate (TSource v) {
                return v;
            }, keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
        {
            return source.ToImmutableDictionary<TSource, TKey, TValue>(keySelector, elementSelector, null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer)
        {
            return source.ToImmutableDictionary<TSource, TKey, TValue>(keySelector, elementSelector, keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<TSource>>(source, "source");
            Requires.NotNull<Func<TSource, TKey>>(keySelector, "keySelector");
            Requires.NotNull<Func<TSource, TValue>>(elementSelector, "elementSelector");
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(Enumerable.Select<TSource, KeyValuePair<TKey, TValue>>(source, delegate (TSource element) {
                return new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element));
            }));
        }
    }
}

