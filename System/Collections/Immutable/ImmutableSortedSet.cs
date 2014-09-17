namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class ImmutableSortedSet
    {
        public static ImmutableSortedSet<T> Create<T>()
        {
            return ImmutableSortedSet<T>.Empty;
        }

        public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer);
        }

        public static ImmutableSortedSet<T> Create<T>(T item)
        {
            return ImmutableSortedSet<T>.Empty.Add(item);
        }

        public static ImmutableSortedSet<T> Create<T>(params T[] items)
        {
            return ImmutableSortedSet<T>.Empty.Union(items);
        }

        public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, T item)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Add(item);
        }

        public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, params T[] items)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
        }

        public static ImmutableSortedSet<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        public static ImmutableSortedSet<T>.Builder CreateBuilder<T>(IComparer<T> comparer)
        {
            return Create<T>(comparer).ToBuilder();
        }

        public static ImmutableSortedSet<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableSortedSet<T>.Empty.Union(items);
        }

        public static ImmutableSortedSet<T> CreateRange<T>(IComparer<T> comparer, IEnumerable<T> items)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
        }

        public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(IEnumerable<TSource> source)
        {
            return ToImmutableSortedSet<TSource>(source, null);
        }

        public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            ImmutableSortedSet<TSource> set = source as ImmutableSortedSet<TSource>;
            if (set != null)
            {
                return set.WithComparer(comparer);
            }
            return ImmutableSortedSet<TSource>.Empty.WithComparer(comparer).Union(source);
        }
    }
}

