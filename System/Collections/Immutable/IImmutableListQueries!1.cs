﻿using System.Collections.Generic.V40;

namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal interface IImmutableListQueries<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        int BinarySearch(T item);
        int BinarySearch(T item, IComparer<T> comparer);
        int BinarySearch(int index, int count, T item, IComparer<T> comparer);
        ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter);
        void CopyTo(T[] array);
        void CopyTo(T[] array, int arrayIndex);
        void CopyTo(int index, T[] array, int arrayIndex, int count);
        bool Exists(Predicate<T> match);
        T Find(Predicate<T> match);
        ImmutableList<T> FindAll(Predicate<T> match);
        int FindIndex(Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(int startIndex, int count, Predicate<T> match);
        T FindLast(Predicate<T> match);
        int FindLastIndex(Predicate<T> match);
        int FindLastIndex(int startIndex, Predicate<T> match);
        int FindLastIndex(int startIndex, int count, Predicate<T> match);
        void ForEach(Action<T> action);
        ImmutableList<T> GetRange(int index, int count);
        bool TrueForAll(Predicate<T> match);
    }
}

