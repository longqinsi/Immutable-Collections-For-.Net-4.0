namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class ImmutableHashSet
    {
        public static ImmutableHashSet<T> Create<T>()
        {
            return ImmutableHashSet<T>.Empty;
        }

        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer);
        }

        public static ImmutableHashSet<T> Create<T>(T item)
        {
            return ImmutableHashSet<T>.Empty.Add(item);
        }

        public static ImmutableHashSet<T> Create<T>(params T[] items)
        {
            return ImmutableHashSet<T>.Empty.Union(items);
        }

        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer, T item)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Add(item);
        }

        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer, params T[] items)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
        }

        public static ImmutableHashSet<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        public static ImmutableHashSet<T>.Builder CreateBuilder<T>(IEqualityComparer<T> equalityComparer)
        {
            return Create<T>(equalityComparer).ToBuilder();
        }

        public static ImmutableHashSet<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableHashSet<T>.Empty.Union(items);
        }

        public static ImmutableHashSet<T> CreateRange<T>(IEqualityComparer<T> equalityComparer, IEnumerable<T> items)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
        }

        public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(IEnumerable<TSource> source)
        {
            return ToImmutableHashSet<TSource>(source, null);
        }

        public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> equalityComparer)
        {
            ImmutableHashSet<TSource> set = source as ImmutableHashSet<TSource>;
            if (set != null)
            {
                return set.WithComparer(equalityComparer);
            }
            return ImmutableHashSet<TSource>.Empty.WithComparer(equalityComparer).Union(source);
        }
    }
}

