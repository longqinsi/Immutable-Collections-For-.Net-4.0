namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IImmutableSet<T> : IReadOnlyCollectionV40<T>, IEnumerable<T>, IEnumerable
    {
        IImmutableSet<T> Add(T value);
        IImmutableSet<T> Clear();
        bool Contains(T value);
        IImmutableSet<T> Except(IEnumerable<T> other);
        IImmutableSet<T> Intersect(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        IImmutableSet<T> Remove(T value);
        bool SetEquals(IEnumerable<T> other);
        IImmutableSet<T> SymmetricExcept(IEnumerable<T> other);
        bool TryGetValue(T equalValue, out T actualValue);
        IImmutableSet<T> Union(IEnumerable<T> other);
    }
}

