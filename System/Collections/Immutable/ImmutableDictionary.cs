using System.Linq;

namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Validation;

    public static class ImmutableDictionary
    {
        public static bool Contains<TKey, TValue>(IImmutableDictionary<TKey, TValue> map, TKey key, TValue value)
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

        public static TValue GetValueOrDefault<TKey, TValue>(IImmutableDictionary<TKey, TValue> dictionaryV40, TKey key)
        {
            return GetValueOrDefault<TKey, TValue>(dictionaryV40, key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(IImmutableDictionary<TKey, TValue> dictionaryV40, TKey key, TValue defaultValue)
        {
            TValue local;
            Requires.NotNull<IImmutableDictionary<TKey, TValue>>(dictionaryV40, "dictionaryV40");
            Requires.NotNullAllowStructs<TKey>(key, "key");
            if (dictionaryV40.TryGetValue(key, out local))
            {
                return local;
            }
            return defaultValue;
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return ToImmutableDictionary<TKey, TValue>(source, null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
        {
            return ToImmutableDictionary<TKey, TValue>(source, keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector)
        {
            return ToImmutableDictionary<TSource, TKey, TSource>(source, keySelector, delegate(TSource v)
            {
                return v;
            }, null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(source, "source");
            ImmutableDictionary<TKey, TValue> dictionaryV40 = source as ImmutableDictionary<TKey, TValue>;
            if (dictionaryV40 != null)
            {
                return dictionaryV40.WithComparers(keyComparer, valueComparer);
            }
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
        }

        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            return ToImmutableDictionary<TSource, TKey, TSource>(source, keySelector, delegate(TSource v)
            {
                return v;
            }, keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TValue> elementSelector)
        {
            return ToImmutableDictionary<TSource, TKey, TValue>(source, keySelector, elementSelector, null, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer)
        {
            return ToImmutableDictionary<TSource, TKey, TValue>(source, keySelector, elementSelector, keyComparer, null);
        }

        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<TSource>>(source, "source");
            Requires.NotNull<FuncV20<TSource, TKey>>(keySelector, "keySelector");
            Requires.NotNull<FuncV20<TSource, TValue>>(elementSelector, "elementSelector");
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(EnumerableV20.Select<TSource, KeyValuePair<TKey, TValue>>(source, delegate (TSource element) {
                return new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element));
            }));
        }
    }
}

