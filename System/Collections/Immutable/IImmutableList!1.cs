namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IImmutableList<T> : IReadOnlyListV40<T>, IReadOnlyCollectionV40<T>, IEnumerable<T>, IEnumerable
    {
        IImmutableList<T> Add(T value);
        IImmutableList<T> AddRange(IEnumerable<T> items);
        IImmutableList<T> Clear();
        int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer);
        IImmutableList<T> Insert(int index, T element);
        IImmutableList<T> InsertRange(int index, IEnumerable<T> items);
        int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer);
        IImmutableList<T> Remove(T value, IEqualityComparer<T> equalityComparer);
        IImmutableList<T> RemoveAll(Predicate<T> match);
        IImmutableList<T> RemoveAt(int index);
        IImmutableList<T> RemoveRange(int index, int count);
        IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer);
        IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer);
        IImmutableList<T> SetItem(int index, T value);
    }
}

