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

        public static int IndexOf<T>(this IImmutableList<T> listV40, T item)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.IndexOf(item, 0, listV40.Count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int IndexOf<T>(this IImmutableList<T> listV40, T item, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.IndexOf(item, 0, listV40.Count, equalityComparer);
        }

        public static int IndexOf<T>(this IImmutableList<T> listV40, T item, int startIndex)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.IndexOf(item, startIndex, listV40.Count - startIndex, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int IndexOf<T>(this IImmutableList<T> listV40, T item, int startIndex, int count)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.IndexOf(item, startIndex, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> listV40, T item)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            if (listV40.Count == 0)
            {
                return -1;
            }
            return listV40.LastIndexOf(item, listV40.Count - 1, listV40.Count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> listV40, T item, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            if (listV40.Count == 0)
            {
                return -1;
            }
            return listV40.LastIndexOf(item, listV40.Count - 1, listV40.Count, equalityComparer);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> listV40, T item, int startIndex)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            if ((listV40.Count == 0) && (startIndex == 0))
            {
                return -1;
            }
            return listV40.LastIndexOf(item, startIndex, startIndex + 1, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static int LastIndexOf<T>(this IImmutableList<T> listV40, T item, int startIndex, int count)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.LastIndexOf(item, startIndex, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> Remove<T>(this IImmutableList<T> listV40, T value)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.Remove(value, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> RemoveRange<T>(this IImmutableList<T> listV40, IEnumerable<T> items)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.RemoveRange(items, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static IImmutableList<T> Replace<T>(this IImmutableList<T> listV40, T oldValue, T newValue)
        {
            Requires.NotNull<IImmutableList<T>>(listV40, "listV40");
            return listV40.Replace(oldValue, newValue, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static ImmutableList<TSource> ToImmutableList<TSource>(this IEnumerable<TSource> source)
        {
            ImmutableList<TSource> listV40 = source as ImmutableList<TSource>;
            if (listV40 != null)
            {
                return listV40;
            }
            return ImmutableList<TSource>.Empty.AddRange(source);
        }
    }
}

