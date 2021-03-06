﻿namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Validation;

    public static class ImmutableSortedDictionary
    {
        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>()
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty;
        }

        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey> keyComparer)
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
        }

        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
        }

        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
        {
            return Create<TKey, TValue>().ToBuilder();
        }

        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey> keyComparer)
        {
            return Create<TKey, TValue>(keyComparer).ToBuilder();
        }

        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            return Create<TKey, TValue>(keyComparer, valueComparer).ToBuilder();
        }

        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.AddRange(items);
        }

        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
        }

        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToImmutableSortedDictionary<TKey, TValue>(null, null);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey> keyComparer)
        {
            return source.ToImmutableSortedDictionary<TKey, TValue>(keyComparer, null);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(source, "source");
            ImmutableSortedDictionary<TKey, TValue> dictionaryV40 = source as ImmutableSortedDictionary<TKey, TValue>;
            if (dictionaryV40 != null)
            {
                return dictionaryV40.WithComparers(keyComparer, valueComparer);
            }
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
        {
            return source.ToImmutableSortedDictionary<TSource, TKey, TValue>(keySelector, elementSelector, null, null);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey> keyComparer)
        {
            return source.ToImmutableSortedDictionary<TSource, TKey, TValue>(keySelector, elementSelector, keyComparer, null);
        }

        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<IEnumerable<TSource>>(source, "source");
            Requires.NotNull<Func<TSource, TKey>>(keySelector, "keySelector");
            Requires.NotNull<Func<TSource, TValue>>(elementSelector, "elementSelector");
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(Enumerable.Select<TSource, KeyValuePair<TKey, TValue>>(source, delegate (TSource element) {
                return new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element));
            }));
        }
    }
}

