namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Validation;

    public static class ImmutableList
    {
        public static ImmutableList<T> Create<T>()
        {
            return ImmutableList<T>.Empty;
        }

        public static ImmutableList<T> Create<T>(T item)
        {
            return ImmutableList<T>.Empty.Add(item);
        }

        public static ImmutableList<T> Create<T>(params T[] items)
        {
            return ImmutableList<T>.Empty.AddRange(items);
        }

        public static ImmutableList<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        public static ImmutableList<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableList<T>.Empty.AddRange(items);
        }

        public static int IndexOf<T>(this IImmutableList<T> list, T item)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, 0, list.Count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int IndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, 0, list.Count, equalityComparer);
        }

        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, startIndex, list.Count - startIndex, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.IndexOf(item, startIndex, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> list, T item)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if (list.Count == 0)
            {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if (list.Count == 0)
            {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, equalityComparer);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            if ((list.Count == 0) && (startIndex == 0))
            {
                return -1;
            }
            return list.LastIndexOf(item, startIndex, startIndex + 1, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.LastIndexOf(item, startIndex, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> Remove<T>(this IImmutableList<T> list, T value)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.Remove(value, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> RemoveRange<T>(this IImmutableList<T> list, IEnumerable<T> items)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.RemoveRange(items, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> Replace<T>(this IImmutableList<T> list, T oldValue, T newValue)
        {
            Requires.NotNull<IImmutableList<T>>(list, "list");
            return list.Replace(oldValue, newValue, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static ImmutableList<TSource> ToImmutableList<TSource>(this IEnumerable<TSource> source)
        {
            ImmutableList<TSource> list = source as ImmutableList<TSource>;
            if (list != null)
            {
                return list;
            }
            return ImmutableList<TSource>.Empty.AddRange(source);
        }
    }
}

