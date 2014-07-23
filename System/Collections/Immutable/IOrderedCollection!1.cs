namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal interface IOrderedCollection<out T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }

        T this[int index] { get; }
    }
}

